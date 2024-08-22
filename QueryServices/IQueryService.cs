using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IQueryService
{
    Task<JToken> GetMPEWatchData(CancellationToken token);
    Task<QuuppaTag> GetQPETagData(CancellationToken token);
    Task<JToken> GetIDSData(CancellationToken token);
    Task<string> SendEmail(CancellationToken stoppingToken);
    Task<JToken> GetSVDoorData(CancellationToken stoppingToken);
    Task<List<(string areaId, string areaName)>> GetAreasAsync(CancellationToken stoppingToken);
    Task<List<AreaDwell>> GetTotalDwellTime(DateTime hour, DateTime dateTime, TimeSpan timeSpan1, TimeSpan timeSpan2,
        TimeSpan timeSpan3, TimeSpan timeSpan4, TimeSpan timeSpan5, List<(string areaId, string areaName)> allAreaIds, int areasBatchCount, CancellationToken stoppingToken);
    Task<List<TagTimeline>> GetTotalTagTimeline(DateTime hour, DateTime dateTime, TimeSpan timeSpan1, TimeSpan timeSpan2,
        TimeSpan timeSpan3, TimeSpan timeSpan4, TimeSpan timeSpan5, List<(string areaId, string areaName)> allAreaIds, int areasBatchCount, CancellationToken stoppingToken);
    Task<JToken> GetSMSWrapperData(CancellationToken stoppingToken);
    Task<JToken> GetIVESData(CancellationToken stoppingToken);
    Task<JToken> GetCiscoSpacesData(CancellationToken stoppingToken);
    Task<JToken> GetCameraData(CancellationToken stoppingToken);
    Task<byte[]> GetPictureData(CancellationToken stoppingToken);
}