using Newtonsoft.Json.Linq;

namespace EIR_9209_2.DatabaseCalls.IDS
{
    public interface IIDS
    {
        Task<JToken> GetOracleIDSData(JToken data);
    }
}