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
                                            command.BindByName = true;
                                            if (((JObject)Request_data).ContainsKey("endDate"))
                                            {
                                                var endDateStr = ((JObject)Request_data)["endDate"]?.ToString();
                                                if (DateTimeOffset.TryParse(endDateStr, out var endDate))
                                                {
                                                    ((JObject)Request_data)["endDate"]?.Replace(new JValue(endDate));
                                                }

                                            }
                                            if (((JObject)Request_data).ContainsKey("startDate"))
                                            {
                                                var startDateStr = ((JObject)Request_data)["startDate"]?.ToString();
                                                if (DateTimeOffset.TryParse(startDateStr, out var startDate))
                                                {
                                                    ((JObject)Request_data)["startDate"]?.Replace(new JValue(startDate));
                                                }
                                            }
                                            if (((JObject)Request_data).ContainsKey("startHour") && ((JObject)Request_data).Type != JTokenType.Integer)
                                            {
                                                int startHour = (int)Request_data["startHour"];
                                                Request_data["startHour"]?.Replace(new JValue(startHour));

                                            }
                                            if (((JObject)Request_data).ContainsKey("endHour") && ((JObject)Request_data).Type != JTokenType.Integer)
                                            {
                                                int startHour = (int)((JObject)Request_data)["endHour"];
                                                ((JObject)Request_data)["endHour"].Replace(new JValue(startHour));

                                            }
                                            foreach (KeyValuePair<string, JToken> property in (JObject)Request_data)
                                            {
                                                if (property.Key != "queryName")
                                                {
                                                    if (!string.IsNullOrEmpty((string)property.Value) && Regex.IsMatch(query, "(\\:" + property.Key + ")", RegexOptions.IgnoreCase))
                                                    {
                                                        if (property.Value.Type == JTokenType.Date)
                                                        {
                                                            command.Parameters.Add(string.Concat(":", property.Key) ?? "", OracleDbType.TimeStamp, (DateTime)property.Value, ParameterDirection.Input);
                                                        }
                                                        if (property.Value.Type == JTokenType.Integer)
                                                        {
                                                            command.Parameters.Add(string.Concat(":", property.Key) ?? "", OracleDbType.Int32, (int)property.Value, ParameterDirection.Input);
                                                        }
                                                        else
                                                        {
                                                            command.Parameters.Add(string.Concat(":", property.Key) ?? "", OracleDbType.Varchar2, property.Value?.ToString(), ParameterDirection.Input);
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
