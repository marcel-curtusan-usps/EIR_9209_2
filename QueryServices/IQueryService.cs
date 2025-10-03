using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IQueryService
{
    Task<JToken> GetMPEWatchData(CancellationToken token);
    Task<QuuppaTag> GetQPETagData(CancellationToken token);
    Task<JToken> GetIDSData(CancellationToken token);
    Task<string> SendEmail(CancellationToken stoppingToken);
    Task<JToken> GetSVDoorData(CancellationToken stoppingToken);
    /// <summary>
    ///     Gets a list of areas with their IDs and names.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    Task<List<(string areaId, string areaName)>> GetAreasAsync(CancellationToken stoppingToken);
    /// <summary>
    ///     Gets a list of areas with their IDs and names.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    Task<List<(string areaId, int originId)>> GetAreasOriginIdAsync(CancellationToken stoppingToken);
    /// <summary>
    ///    Gets a list of badge IDs and tag IDs.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    Task<List<(string employeeId, string tagId)>> GetBadgeListAsync(CancellationToken stoppingToken);
    /// <summary>
    ///     Gets a list of all badge IDs and tag IDs.
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="dateTime"></param>
    /// <param name="timeSpan1"></param>
    /// <param name="timeSpan2"></param>
    /// <param name="timeSpan3"></param>
    /// <param name="timeSpan4"></param>
    /// <param name="timeSpan5"></param>
    /// <param name="allAreaIds"></param>
    /// <param name="areasBatchCount"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    Task<List<AreaDwell>> GetTotalDwellTime(DateTime hour, DateTime dateTime, TimeSpan timeSpan1, TimeSpan timeSpan2,
        TimeSpan timeSpan3, TimeSpan timeSpan4, TimeSpan timeSpan5, List<(string areaId, string areaName)> allAreaIds, int areasBatchCount, CancellationToken stoppingToken);
    /// <summary>
    ///     Gets the total dwell time for a specific hour and date range.
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="dateTime"></param>
    /// <param name="timeSpan1"></param>
    /// <param name="timeSpan2"></param>
    /// <param name="timeSpan3"></param>
    /// <param name="timeSpan4"></param>
    /// <param name="timeSpan5"></param>
    /// <param name="allBadgeIds"></param>
    /// <param name="allAreaIds"></param>
    /// <param name="areasBatchCount"></param>
    /// <param name="eventTypes"></param>
    /// <param name="webhookUrl"></param>
    /// <param name="webhookUserName"></param>
    /// <param name="webhookPassword"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    Task<ReportResponse> CreateReportDwellTime(DateTime hour, DateTime dateTime, TimeSpan timeSpan1, TimeSpan timeSpan2,
        TimeSpan timeSpan3, TimeSpan timeSpan4, TimeSpan timeSpan5, List<(string employeeId, string tagId)> allBadgeIds, List<(string areaId, int originId)> allAreaIds, int areasBatchCount, List<string> eventTypes,
        string webhookUrl, string webhookUserName, string webhookPassword, CancellationToken stoppingToken);
    /// <summary>
    /// Gets the total tag timeline for a specific hour and date range.
    /// </summary>
    /// <param name="hour"></param>
    /// <param name="dateTime"></param>
    /// <param name="timeSpan1"></param>
    /// <param name="timeSpan2"></param>
    /// <param name="timeSpan3"></param>
    /// <param name="timeSpan4"></param>
    /// <param name="timeSpan5"></param>
    /// <param name="allAreaIds"></param>
    /// <param name="areasBatchCount"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    Task<List<TagTimeline>> GetTotalTagTimeLine(DateTime hour, DateTime dateTime, TimeSpan timeSpan1, TimeSpan timeSpan2,
        TimeSpan timeSpan3, TimeSpan timeSpan4, TimeSpan timeSpan5, List<(string areaId, string areaName)> allAreaIds, int areasBatchCount, CancellationToken stoppingToken);

    /// <summary>
    /// Downloads the report dwell time for a specific report ID.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    Task<List<ReportContentItems>> DownloadReportDwellTime(string reportId, CancellationToken stoppingToken);
    /// <summary>
    ///     Gets the total tag timeline for a specific hour and date range.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    Task<List<SMSWrapperEmployeeInfo>> GetSMSWrapperData(CancellationToken stoppingToken);
    Task<JToken> GetSMSWrapperDBData(CancellationToken stoppingToken);
    Task<JToken> GetIVESData(CancellationToken stoppingToken);
    Task<JToken> GetCiscoSpacesData(CancellationToken stoppingToken);
    Task<JToken> GetCameraData(CancellationToken stoppingToken);
    Task<byte[]> GetPictureData(CancellationToken stoppingToken);
    Task<QPEProjectInfo> GetQPEProjectInfo(CancellationToken stoppingToken);
    Task<Hces> GetHCESData(CancellationToken stoppingToken, string facilityId, string oAuthClientId, string oAuthClientId1);
    Task<MpeWatchRequestId> GetMPEWatchRequestId(CancellationToken stoppingToken);
    Task<JToken> GetMapElementsAsync(string elemnetsUrl, CancellationToken stoppingToken);
    Task<string> GetMapImageAsync(string imageUrl, CancellationToken stoppingToken);
}