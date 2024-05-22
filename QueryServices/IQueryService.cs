using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IQueryService
{
    Task<JToken> GetMPEWatchData(CancellationToken token);
    Task<QuuppaTag> GetQuuppaTagData(CancellationToken token);
    Task<JToken> GetIDSData(string messageType, int hoursBack, int hoursForward, CancellationToken token);
}