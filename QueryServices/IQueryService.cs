using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IQueryService
{
    Task<JToken> GetMPEWatchData(CancellationToken token);
    Task<QuuppaTag> GetQPETagData(CancellationToken token);
    Task<JToken> GetIDSData(CancellationToken token);
    Task<string> SendEmail(CancellationToken stoppingToken);
    Task<JToken> GetSVDoorData(CancellationToken stoppingToken);
}