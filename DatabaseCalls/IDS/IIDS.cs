using Newtonsoft.Json.Linq;

namespace EIR_9209_2.DatabaseCalls.IDS
{
    /// <summary>
    /// IDS Interface
    /// </summary>
    public interface IIDS
    {
        /// <summary>
        ///  Get Oracle IDS Data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<(object?, object?)> GetOracleIDSData(JToken data);
    }
}