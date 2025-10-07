using Newtonsoft.Json.Linq;

namespace EIR_9209_2.DatabaseCalls.IDS
{
    public interface IIDS
    {
        Task<(JObject?, JToken)> GetOracleIDSData(JToken data);
    }
}