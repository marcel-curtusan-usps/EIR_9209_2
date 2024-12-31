using EIR_9209_2.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text.RegularExpressions;

namespace EIR_9209_2.DatabaseCalls.IDS
{
    public class IDS(ILogger<IDS> logger, IConfiguration configuration, IFileService fileService, IEncryptDecrypt encryptDecrypt) : IIDS
    {
        private readonly ILogger<IDS> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private string query = string.Empty;
        private string OracleConnectionString = string.Empty;
        private JToken result = new JObject();
        private readonly IFileService _fileService = fileService;
        private readonly IEncryptDecrypt _encryptDecrypt = encryptDecrypt;

        public async Task<JToken> GetOracleIDSData(JToken Request_data)
        {
            try
            {
                OracleConnectionString = _configuration[key: "ApplicationConfiguration:IdsConnectionString"];
                if (!string.IsNullOrEmpty(OracleConnectionString))
                {
                    if (((JObject)Request_data).ContainsKey("queryName"))
                    {
                        if (!string.IsNullOrEmpty(Request_data["queryName"]?.ToString()))
                        {
                            string directory = Path.Combine(_configuration[key: "ApplicationConfiguration:OracleQueryDirectory"], "IDS");
                            var fileName = $"{Request_data["queryName"]?.ToString()}.txt";
                            query = await _fileService.ReadFileFromRoot(fileName, directory);
                            if (!string.IsNullOrEmpty(query))
                            {
                                using (OracleConnection connection = new OracleConnection(_encryptDecrypt.Decrypt(OracleConnectionString)))
                                {
                                    using (OracleCommand command = new OracleCommand(query, connection))
                                    {
                                        try
                                        {
                                            if (connection.State == ConnectionState.Closed)
                                            {
                                                connection.Open();
                                            }
                                            command.Parameters.Clear();
                                            command.BindByName = false;
                                    
                                            foreach (KeyValuePair<string, JToken> property in (JObject)Request_data)
                                            {
                                                if (property.Key != "queryName")
                                                {
                                                    if (property.Key.Equals("datadayList", StringComparison.CurrentCultureIgnoreCase) || property.Key.Equals("rejectBins", StringComparison.CurrentCultureIgnoreCase))
                                                    {
                                                        if (property.Value.Type == JTokenType.Array && property.Value is JArray array && array.All(item => item.Type == JTokenType.Integer))
                                                        {
                                                            string intArrayString = string.Join(",", array.Select(item => item.ToString()));
                                                            command.CommandText = query = query.Replace($":{property.Key.ToUpper()}", intArrayString);
                                                        }
                                                    }
                                                }
                                            }
                                            OracleDataReader odr = command.ExecuteReader();
                                            if (odr.HasRows)
                                            {
                                                // Create DataTable to store query results
                                                DataTable dt = new DataTable();
                                                dt.Load(odr);
                                                // Convert DataTable to JSON string
                                                string jsonResult = JsonConvert.SerializeObject(dt, Formatting.Indented);
                                                // Check if query returned a single row or multiple rows
                                                bool isSingleRow = dt.Rows.Count == 1;
                                                // Deserialize JSON string to JObject or JArray
                                                result = isSingleRow ? JToken.Parse(jsonResult) : JArray.Parse(jsonResult);
                                            }
                                            odr.Close();
                                            return result;
                                        }
                                        catch (OracleException oe)
                                        {
                                            return new JObject
                                            {
                                                ["Error"] = string.Concat("Oracle error: ", oe.Message),
                                                ["Code"] = "5"
                                            };
                                        }

                                    }
                                }
                            }
                            else
                            {
                                return new JObject
                                {
                                    ["Error"] = string.Concat("File Content is Empty for Filename: ", Request_data["queryName"]?.ToString()),
                                    ["Code"] = "4"
                                };
                            }
                        }
                        else
                        {
                            return new JObject
                            {
                                ["Error"] = string.Concat("No QueryName Attribute is empty ", Request_data["queryName"]?.ToString()),
                                ["Code"] = "3"
                            };
                        }
                    }
                    else
                    {
                        return new JObject
                        {
                            ["Error"] = string.Concat("No QueryName Attribute found name "),
                            ["Code"] = "3"
                        };
                    }
                }
                else
                {
                    return new JObject
                    {
                        ["Error"] = "Connection not Valid",
                        ["Code"] = "1"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new JObject
                {
                    ["Error"] = string.Concat("Error: ", ex.Message),
                    ["Code"] = "6"
                };
            }

        }
    }
}
