using EIR_9209_2.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace EIR_9209_2.DatabaseCalls.MPE
{
    public class Mpe(ILogger<Mpe> logger, IConfiguration configuration, IFileService fileService, IEncryptDecrypt encryptDecrypt) : IMpe
    {
        private readonly ILogger<Mpe> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly IFileService _fileService = fileService;
        private readonly IEncryptDecrypt _encryptDecrypt = encryptDecrypt;

        /// <summary>
        /// Load SQL from file, apply parameter replacements and execute against Oracle.
        /// Accepts parameters in the incoming JToken (queryName, idsConnectionString, other named parameters).
        /// Array-valued properties (JArray) will be expanded inline (comma-separated) so they work with IN (...) clauses.
        /// Scalar properties will be bound as named parameters to avoid SQL injection and ORA-01008 errors.
        /// </summary>
        public async Task<(object?, object?)> GetMpeData(JToken data)
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
                return await ExecuteQueryToJTokenAsync<object>(rawQuery, OracleConnectionString, sqlParamNames, data);

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
        private static async Task<(T? status, T? result)> ExecuteQueryToJTokenAsync<T>(string queryText, string connectionString, HashSet<string> parameters, JToken requestData)
        {
            var metadata = new JObject();
            JToken? result = null;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            metadata["StartTimeUtc"] = DateTime.UtcNow.ToString("o");

            TryCaptureQueryName(requestData, metadata);
            var parametersUsed = new JObject();

            try
            {
                using var connection = new OracleConnection(connectionString);
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = queryText;
                command.BindByName = true;

                AddParameters(command, parameters, requestData, parametersUsed);

                TryAddFinalQueryMetadata(command, metadata, parametersUsed);

                using var reader = await command.ExecuteReaderAsync();
                result = ReadResultsFromReader(reader);

                stopwatch.Stop();
                FinalizeSuccessMetadata(metadata, stopwatch);
                return ((T?)(object)metadata, (T?)(object)result);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                FinalizeErrorMetadata(metadata, stopwatch, ex);
                return ((T?)(object)metadata, default);
            }
        }

        private static void TryCaptureQueryName(JToken requestData, JObject metadata)
        {
            try
            {
                if (requestData is not JObject rqObj) return;
                foreach (var p in rqObj.Properties())
                {
                    if (NormalizeKey(p.Name) == "QUERYNAME")
                    {
                        metadata["QueryName"] = p.Value?.ToString();
                        break;
                    }
                }
            }
            catch { /* ignore */ }
        }

        private static void AddParameters(OracleCommand command, HashSet<string> parameters, JToken requestData, JObject parametersUsed)
        {
            if (requestData is not JObject reqObj) return;

            foreach (var pname in parameters)
            {
                foreach (KeyValuePair<string, JToken?> property in reqObj)
                {
                    if (NormalizeKey(property.Key) != NormalizeKey(pname)) continue;

                    if (!parametersUsed.ContainsKey(property.Key))
                    {
                        parametersUsed[property.Key] = property.Value ?? JValue.CreateNull();
                    }

                    // Arrays -> CSV string
                    if (property.Value != null && property.Value.Type == JTokenType.Array && property.Value is JArray arr)
                    {
                        string csv = string.Join(",", arr.Select(item => item.ToString()));
                        AddOrReplaceParameter(command, NormalizeKey(property.Key), OracleDbType.Varchar2, csv);
                    }
                    else if (property.Value != null && property.Value.Type == JTokenType.String)
                    {
                        AddOrReplaceParameter(command, NormalizeKey(property.Key), OracleDbType.Varchar2, property.Value.ToString());
                    }
                    else if (property.Value != null && (property.Value.Type == JTokenType.Integer || property.Value.Type == JTokenType.Float || property.Value.Type == JTokenType.Boolean))
                    {
                        if (property.Value.Type == JTokenType.Integer)
                        {
                            AddOrReplaceParameter(command, NormalizeKey(property.Key), OracleDbType.Int32, property.Value.ToObject<int>());
                        }
                        else if (property.Value.Type == JTokenType.Float)
                        {
                            AddOrReplaceParameter(command, NormalizeKey(property.Key), OracleDbType.Decimal, property.Value.ToObject<decimal>());
                        }
                        else // boolean
                        {
                            var boolVal = property.Value.ToObject<bool>() ? (short)1 : (short)0;
                            AddOrReplaceParameter(command, NormalizeKey(property.Key), OracleDbType.Int16, boolVal);
                        }
                    }
                }
            }
        }

        private static void AddOrReplaceParameter(OracleCommand command, string paramName, OracleDbType dbType, object? value)
        {
            var oracleParam = new OracleParameter(paramName, dbType)
            {
                Direction = ParameterDirection.Input,
                Value = value ?? DBNull.Value
            };
            if (command.Parameters.Contains(paramName)) command.Parameters.Remove(paramName);
            command.Parameters.Add(oracleParam);
        }

        private static void TryAddFinalQueryMetadata(OracleCommand command, JObject metadata, JObject parametersUsed)
        {
            try
            {
                metadata["FinalQuery"] = command.CommandText;
                metadata["Parameters"] = parametersUsed.ToString();
            }
            catch { /* ignore metadata errors */ }
        }

        private static JToken? ReadResultsFromReader(System.Data.Common.DbDataReader reader)
        {
            if (!reader.HasRows) return null;
            DataTable dt = new DataTable();
            dt.Load(reader);
            string jsonResult = JsonConvert.SerializeObject(dt, Formatting.Indented);
            bool isSingleRow = dt.Rows.Count == 1;
            return isSingleRow ? JToken.Parse(jsonResult) : JArray.Parse(jsonResult);
        }

        private static void FinalizeSuccessMetadata(JObject metadata, System.Diagnostics.Stopwatch stopwatch)
        {
            var end = DateTime.UtcNow;
            metadata["EndTimeUtc"] = end.ToString("o");
            metadata["DurationMilliseconds"] = stopwatch.Elapsed.TotalMilliseconds;
            metadata["Status"] = "Success";
        }

        private static void FinalizeErrorMetadata(JObject metadata, System.Diagnostics.Stopwatch stopwatch, Exception ex)
        {
            var end = DateTime.UtcNow;
            metadata["EndTimeUtc"] = end.ToString("o");
            metadata["DurationMilliseconds"] = stopwatch.Elapsed.TotalMilliseconds;
            metadata["Status"] = null;
            metadata["Error"] = ex.Message;
        }
        private static HashSet<string> ExtractParameterNames(string sql)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(sql)) return result;

            int i = 0;
            int len = sql.Length;

            while (i < len)
            {
                char c = sql[i];

                // If we encounter start of line or block comment, skip it
                if (IsStartOfLineComment(sql, i))
                {
                    i = SkipLineComment(sql, i + 2);
                    continue;
                }
                if (IsStartOfBlockComment(sql, i))
                {
                    i = SkipBlockComment(sql, i + 2);
                    continue;
                }

                // Toggle quotes and skip their content
                if (c == '\'' && !IsDoubleQuoteOpen(sql, i))
                {
                    i = SkipSingleQuote(sql, i + 1);
                    continue;
                }
                if (c == '"' && !IsSingleQuoteOpen(sql, i))
                {
                    i = SkipDoubleQuote(sql, i + 1);
                    continue;
                }

                // Collect parameter after colon when not inside quotes/comments
                if (c == ':')
                {
                    int next = CollectParameterName(sql, i + 1, out var name);
                    if (!string.IsNullOrEmpty(name)) result.Add(name);
                    i = Math.Max(i + 1, next);
                    continue;
                }

                i++;
            }

            return result;
        }

        private static bool IsStartOfLineComment(string s, int i)
        {
            return i + 1 < s.Length && s[i] == '-' && s[i + 1] == '-';
        }

        private static bool IsStartOfBlockComment(string s, int i)
        {
            return i + 1 < s.Length && s[i] == '/' && s[i + 1] == '*';
        }

        private static int SkipLineComment(string s, int index)
        {
            while (index < s.Length && s[index] != '\n' && s[index] != '\r') index++;
            return index;
        }

        private static int SkipBlockComment(string s, int index)
        {
            while (index < s.Length - 1)
            {
                if (s[index] == '*' && s[index + 1] == '/') return index + 2;
                index++;
            }
            return index;
        }

        private static int SkipSingleQuote(string s, int index)
        {
            while (index < s.Length)
            {
                if (s[index] == '\'') return index + 1;
                index++;
            }
            return index;
        }

        private static int SkipDoubleQuote(string s, int index)
        {
            while (index < s.Length)
            {
                if (s[index] == '"') return index + 1;
                index++;
            }
            return index;
        }

        // helpers to assist quote logic but keep ExtractParameterNames simple
        private static bool IsSingleQuoteOpen(string s, int index) => false;
        private static bool IsDoubleQuoteOpen(string s, int index) => false;

        private static int CollectParameterName(string s, int index, out string name)
        {
            var sb = new System.Text.StringBuilder();
            while (index < s.Length)
            {
                char nc = s[index];
                if (char.IsLetterOrDigit(nc) || nc == '_' || nc == '$' || nc == '#')
                {
                    sb.Append(nc);
                    index++;
                }
                else break;
            }
            name = sb.ToString();
            return index;
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
    }
}
