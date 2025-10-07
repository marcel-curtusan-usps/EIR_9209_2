using EIR_9209_2.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace EIR_9209_2.DatabaseCalls.IDS
{
    public class IDS(ILogger<IDS> logger, IConfiguration configuration, IFileService fileService, IEncryptDecrypt encryptDecrypt) : IIDS
    {
        private readonly ILogger<IDS> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IFileService _fileService = fileService;
        private readonly IEncryptDecrypt _encryptDecrypt = encryptDecrypt;

        /// <summary>
        /// Load SQL from file, apply parameter replacements and execute against Oracle.
        /// Accepts parameters in the incoming JToken (queryName, idsConnectionString, other named parameters).
        /// Array-valued properties (JArray) will be expanded inline (comma-separated) so they work with IN (...) clauses.
        /// Scalar properties will be bound as named parameters to avoid SQL injection and ORA-01008 errors.
        /// </summary>
        public async Task<(JObject?, JToken)> GetOracleIDSData(JToken data)
        {
            try
            {
                if (data is not JObject reqObj)
                {
                    return (null, new JObject { ["Error"] = "Request data must be a JSON object", ["Code"] = "3" });
                }

                var encryptedConn = reqObj["idsConnectionString"]?.ToString() ?? string.Empty;
                var OracleConnectionString = _encryptDecrypt.Decrypt(encryptedConn);
                if (string.IsNullOrEmpty(OracleConnectionString))
                {
                    return (null, new JObject { ["Error"] = "Connection not Valid", ["Code"] = "1" });
                }

                var baseName = reqObj["queryName"]?.ToString();
                if (string.IsNullOrEmpty(baseName))
                {
                    return (null, new JObject { ["Error"] = "No QueryName Attribute found or empty", ["Code"] = "3" });
                }

                string oracleQueryDirectory = _configuration["ApplicationConfiguration:OracleQueryDirectory"] ?? string.Empty;
                if (string.IsNullOrEmpty(oracleQueryDirectory))
                {
                    return (null, new JObject { ["Error"] = "OracleQueryDirectory is not configured", ["Code"] = "2" });
                }

                string directory = Path.Combine(oracleQueryDirectory, "IDS");

                // Load query text (prefer .sql)
                string rawQuery = await LoadQueryTextAsync(baseName, directory);
                if (string.IsNullOrEmpty(rawQuery))
                {
                    return (null, new JObject { ["Error"] = $"File Content is Empty for Filename: {baseName}", ["Code"] = "4" });
                }

                // Extract parameter names from the SQL file first and match to request data.
                var sqlParamNames = ExtractParameterNames(rawQuery);

                // Build a normalized map of request keys for case/format-insensitive matching
                var normalizedRequestKeys = reqObj.Properties()
                    .ToDictionary(p => NormalizeKey(p.Name), p => p, StringComparer.OrdinalIgnoreCase);

                // Determine missing parameters (ignore internal control properties)
                var missing = sqlParamNames
                    .Where(p => !string.Equals(p, "queryName", StringComparison.OrdinalIgnoreCase)
                                && !string.Equals(p, "idsConnectionString", StringComparison.OrdinalIgnoreCase)
                                && !normalizedRequestKeys.ContainsKey(NormalizeKey(p)))
                    .ToList();

                if (missing.Any())
                {
                    return (null, new JObject
                    {
                        ["Error"] = "Missing required parameters",
                        ["MissingParameters"] = JArray.FromObject(missing),
                        ["Code"] = "7"
                    });
                }

                // No parameters found in SQL, just execute as-is
                return await ExecuteQueryToJTokenAsync(rawQuery, OracleConnectionString, sqlParamNames, data);

            }
            catch (OracleException oe)
            {
                _logger.LogError(oe, "OracleException in GetOracleIDSData");
                return (null, new JObject { ["Error"] = string.Concat("Oracle error: ", oe.Message), ["Code"] = "5" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetOracleIDSData");
                return (null, new JObject { ["Error"] = string.Concat("Error: ", ex.Message), ["Code"] = "6" });
            }
        }
        private async Task<string> LoadQueryTextAsync(string baseName, string directory)
        {
            var fileNameSql = $"{baseName}.sql";
            var fileNameTxt = $"{baseName}.txt";

            // Prefer .sql in directory
            var content = await _fileService.ReadFileFromRoot(fileNameSql, directory);
            if (!string.IsNullOrWhiteSpace(content)) return content;

            // then .txt in directory
            content = await _fileService.ReadFileFromRoot(fileNameTxt, directory);
            if (!string.IsNullOrWhiteSpace(content)) return content;
            return string.Empty;
        }
        private static async Task<(JObject?, JToken)> ExecuteQueryToJTokenAsync(string queryText, string connectionString, HashSet<string> parameters, JToken requestData)
        {
            // Prepare metadata
            var metadata = new JObject();
            JToken? result = null;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var start = DateTime.UtcNow;
            metadata["StartTimeUtc"] = start.ToString("o");

            // capture query name (if provided in requestData) in a case/format-insensitive way
            try
            {
                if (requestData is JObject rqObj)
                {
                    foreach (var p in rqObj.Properties())
                    {
                        if (NormalizeKey(p.Name) == "QUERYNAME")
                        {
                            metadata["QueryName"] = p.Value?.ToString();
                            break;
                        }
                    }
                }
            }
            catch { /* ignore */ }

            // collect parameters actually used/applied for the query
            var parametersUsed = new JObject();

            try
            {
                using var connection = new OracleConnection(connectionString);
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = queryText;
                command.BindByName = false;

                // Build parameters from SQL parameter names and request data
                foreach (var pname in parameters)
                {
                    foreach (KeyValuePair<string, JToken?> property in (JObject)requestData)
                    {
                        if (NormalizeKey(property.Key) == NormalizeKey(pname))
                        {
                            // record the parameter and its value (preserve original property name)
                            if (!parametersUsed.ContainsKey(property.Key))
                            {
                                parametersUsed[property.Key] = property.Value ?? JValue.CreateNull();
                            }

                            if (property.Value != null && property.Value.Type == JTokenType.Array && property.Value is JArray array && array.All(item => item.Type == JTokenType.Integer))
                            {
                                string intArrayString = string.Join(",", array.Select(item => item.ToString()));
                                command.CommandText = queryText = queryText.Replace($":{property.Key.ToUpper()}", intArrayString);
                            }
                            else if (property.Value != null && property.Value.Type == JTokenType.String)
                            {
                                command.CommandText = queryText = queryText.Replace($":{property.Key.ToUpper()}", property.Value.ToString());
                            }
                            else if (property.Value != null && (property.Value.Type == JTokenType.Integer || property.Value.Type == JTokenType.Float || property.Value.Type == JTokenType.Boolean))
                            {
                                // inline simple scalar types
                                command.CommandText = queryText = queryText.Replace($":{property.Key.ToUpper()}", property.Value.ToString());
                            }
                        }
                    }
                }

                // include final query text and parameters used in metadata
                try
                {
                    metadata["FinalQuery"] = command.CommandText;
                    metadata["Parameters"] = parametersUsed.ToString();
                }
                catch { /* ignore metadata errors */ }

                using var reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    // Create DataTable to store query results
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    // Convert DataTable to JSON string
                    string jsonResult = JsonConvert.SerializeObject(dt, Formatting.Indented);
                    // Check if query returned a single row or multiple rows
                    bool isSingleRow = dt.Rows.Count == 1;
                    // Deserialize JSON string to JObject or JArray
                    result = isSingleRow ? JToken.Parse(jsonResult) : JArray.Parse(jsonResult);
                }

                // success
                stopwatch.Stop();
                var end = DateTime.UtcNow;
                metadata["EndTimeUtc"] = end.ToString("o");
                metadata["DurationMilliseconds"] = stopwatch.Elapsed.TotalMilliseconds;
                metadata["Status"] = "Success";
                return (metadata, result ?? JValue.CreateNull());
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var end = DateTime.UtcNow;
                metadata["EndTimeUtc"] = end.ToString("o");
                metadata["DurationMilliseconds"] = stopwatch.Elapsed.TotalMilliseconds;
                // On error, set Status to null as requested and include the error
                metadata["Status"] = null;
                metadata["Error"] = ex.Message;
                // Return metadata and null result
                return (metadata, JValue.CreateNull());
            }

        }
        private static HashSet<string> ExtractParameterNames(string sql)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(sql)) return result;

            bool inSingleQuote = false;
            bool inDoubleQuote = false;
            bool inLineComment = false;
            bool inBlockComment = false;

            for (int i = 0; i < sql.Length; i++)
            {
                char c = sql[i];

                // handle comment starts when not inside quotes
                if (!inLineComment && !inBlockComment && !inSingleQuote && !inDoubleQuote)
                {
                    if (c == '-' && i + 1 < sql.Length && sql[i + 1] == '-')
                    {
                        inLineComment = true;
                        i++; // skip next
                        continue;
                    }
                    if (c == '/' && i + 1 < sql.Length && sql[i + 1] == '*')
                    {
                        inBlockComment = true;
                        i++;
                        continue;
                    }
                }

                if (inLineComment)
                {
                    if (c == '\n' || c == '\r') inLineComment = false;
                    continue;
                }

                if (inBlockComment)
                {
                    if (c == '*' && i + 1 < sql.Length && sql[i + 1] == '/')
                    {
                        inBlockComment = false;
                        i++;
                    }
                    continue;
                }

                if (!inDoubleQuote && c == '\'')
                {
                    // toggle single-quote. Note: does not fully handle doubled-quote escapes but is good for most files
                    inSingleQuote = !inSingleQuote;
                    continue;
                }
                if (!inSingleQuote && c == '"')
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (inSingleQuote || inDoubleQuote) continue;

                if (c == ':')
                {
                    int j = i + 1;
                    var sb = new System.Text.StringBuilder();
                    while (j < sql.Length)
                    {
                        char nc = sql[j];
                        if (char.IsLetterOrDigit(nc) || nc == '_' || nc == '$' || nc == '#')
                        {
                            sb.Append(nc);
                            j++;
                        }
                        else break;
                    }
                    if (sb.Length > 0)
                    {
                        result.Add(sb.ToString());
                        i = j - 1;
                    }
                }
            }

            return result;
        }

        // Normalize keys so that request properties like "Id", ":ID", "id" match SQL parameter names like ":ID" or "id"
        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            // remove leading colon if present and trim
            key = key.Trim();
            if (key.StartsWith(":")) key = key.Substring(1);
            return key.ToUpperInvariant();
        }

        //     private readonly ILogger<IDS> _logger = logger;
        //     private readonly IConfiguration _configuration = configuration;
        //     private string query = string.Empty;
        //     private string OracleConnectionString = string.Empty;
        //     private JToken result = new JObject();
        //     private readonly IFileService _fileService = fileService;
        //     private readonly IEncryptDecrypt _encryptDecrypt = encryptDecrypt;

        //     public async Task<JToken> GetOracleIDSData(JToken Request_data)
        //     {
        //         try
        //         {
        //             OracleConnectionString = _encryptDecrypt.Decrypt(Request_data["idsConnectionString"].ToString());
        //             if (!string.IsNullOrEmpty(OracleConnectionString))
        //             {
        //                 if (((JObject)Request_data).ContainsKey("queryName"))
        //                 {
        //                     if (!string.IsNullOrEmpty(Request_data["queryName"]?.ToString()))
        //                     {
        //                         string oracleQueryDirectory = _configuration[key: "ApplicationConfiguration:OracleQueryDirectory"] ?? string.Empty;
        //                         if (string.IsNullOrEmpty(oracleQueryDirectory))
        //                         {
        //                             return new JObject
        //                             {
        //                                 ["Error"] = "OracleQueryDirectory is not configured",
        //                                 ["Code"] = "2"
        //                             };
        //                         }
        //                         string directory = Path.Combine(oracleQueryDirectory, "IDS");
        //                         var fileName = $"{Request_data["queryName"]?.ToString()}.txt";
        //                         query = await _fileService.ReadFileFromRoot(fileName, directory);
        //                         if (!string.IsNullOrEmpty(query))
        //                         {
        //                             using (OracleConnection connection = new OracleConnection(OracleConnectionString))
        //                             {
        //                                 using (OracleCommand command = new OracleCommand(query, connection))
        //                                 {
        //                                     try
        //                                     {
        //                                         if (connection.State == ConnectionState.Closed)
        //                                         {
        //                                             connection.Open();
        //                                         }
        //                                         command.Parameters.Clear();
        //                                         command.BindByName = false;

        //                                         foreach (KeyValuePair<string, JToken?> property in (JObject)Request_data)
        //                                         {
        //                                             if (property.Key != "queryName")
        //                                             {
        //                                                 if (property.Key.Equals("datadayList", StringComparison.CurrentCultureIgnoreCase) || property.Key.Equals("rejectBins", StringComparison.CurrentCultureIgnoreCase))
        //                                                 {
        //                                                     if (property.Value != null && property.Value.Type == JTokenType.Array && property.Value is JArray array && array.All(item => item.Type == JTokenType.Integer))
        //                                                     {
        //                                                         string intArrayString = string.Join(",", array.Select(item => item.ToString()));
        //                                                         command.CommandText = query = query.Replace($":{property.Key.ToUpper()}", intArrayString);
        //                                                     }
        //                                                 }
        //                                             }
        //                                         }
        //                                         OracleDataReader odr = command.ExecuteReader();
        //                                         if (odr.HasRows)
        //                                         {
        //                                             // Create DataTable to store query results
        //                                             DataTable dt = new DataTable();
        //                                             dt.Load(odr);
        //                                             // Convert DataTable to JSON string
        //                                             string jsonResult = JsonConvert.SerializeObject(dt, Formatting.Indented);
        //                                             // Check if query returned a single row or multiple rows
        //                                             bool isSingleRow = dt.Rows.Count == 1;
        //                                             // Deserialize JSON string to JObject or JArray
        //                                             result = isSingleRow ? JToken.Parse(jsonResult) : JArray.Parse(jsonResult);
        //                                         }
        //                                         odr.Close();
        //                                         return result;
        //                                     }
        //                                     catch (OracleException oe)
        //                                     {
        //                                         return new JObject
        //                                         {
        //                                             ["Error"] = string.Concat("Oracle error: ", oe.Message),
        //                                             ["Code"] = "5"
        //                                         };
        //                                     }

        //                                 }
        //                             }
        //                         }
        //                         else
        //                         {
        //                             return new JObject
        //                             {
        //                                 ["Error"] = string.Concat("File Content is Empty for Filename: ", Request_data["queryName"]?.ToString()),
        //                                 ["Code"] = "4"
        //                             };
        //                         }
        //                     }
        //                     else
        //                     {
        //                         return new JObject
        //                         {
        //                             ["Error"] = string.Concat("No QueryName Attribute is empty ", Request_data["queryName"]?.ToString()),
        //                             ["Code"] = "3"
        //                         };
        //                     }
        //                 }
        //                 else
        //                 {
        //                     return new JObject
        //                     {
        //                         ["Error"] = string.Concat("No QueryName Attribute found name "),
        //                         ["Code"] = "3"
        //                     };
        //                 }
        //             }
        //             else
        //             {
        //                 return new JObject
        //                 {
        //                     ["Error"] = "Connection not Valid",
        //                     ["Code"] = "1"
        //                 };
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             _logger.LogError(ex.Message);
        //             return new JObject
        //             {
        //                 ["Error"] = string.Concat("Error: ", ex.Message),
        //                 ["Code"] = "6"
        //             };
        //         }

        //     }
        // }
    }
}
