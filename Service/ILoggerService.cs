using Newtonsoft.Json.Linq;

namespace EIR_9209_2.Service
{
    public interface ILoggerService
    {
        Task LogData(JToken result, string messageType, string name, string formatUrl);
    }
}