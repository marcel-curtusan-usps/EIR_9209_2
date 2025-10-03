
using EIR_9209_2.Models;
using System.ComponentModel.DataAnnotations;
namespace EIR_9209_2.QueryServices;
/// <summary>
/// Builder class for creating report queries.
/// </summary>
public class ReportQueryBatchProcessCreationBuilder
{
    private readonly ReportRequest _query = new ReportRequest();
/// <summary>
/// evnts
/// </summary>
/// <param name="value"></param>
/// <returns></returns>
    public ReportQueryBatchProcessCreationBuilder WithEvents(List<string> value)
    {

        _query.Events = value;
        return this;
    }
/// <summary>
/// Sets the minimum time on area for the report query.
/// </summary>
/// <param name="value"></param>
/// <returns></returns>
    public ReportQueryBatchProcessCreationBuilder WithMinTimeOnArea(TimeSpan value)
    {
        _query.AreaConfiguration.ConfigParameters.MinTimeOnArea = value;
        return this;
    }
/// <summary>
///     Sets the time step for the report query.
/// </summary>
/// <param name="value"></param>
/// <returns></returns>
    public ReportQueryBatchProcessCreationBuilder WithTimeStep(TimeSpan value)
    {
        _query.AreaConfiguration.ConfigParameters.TimeStep = value;
        return this;
    }
    /// <summary>
    ///     Sets the activation time for the report query.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ReportQueryBatchProcessCreationBuilder WithActivationTime(TimeSpan value)
    {
        _query.AreaConfiguration.ConfigParameters.ActivationTime = value;
        return this;
    }
    /// <summary>
    /// Sets the deactivation time for the report query.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ReportQueryBatchProcessCreationBuilder WithDeactivationTime(TimeSpan value)
    {
        _query.AreaConfiguration.ConfigParameters.DeactivationTime = value;
        return this;
    }
    /// <summary>
    /// Sets the disappear time for the report query.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ReportQueryBatchProcessCreationBuilder WithDisappearTime(TimeSpan value)
    {
        _query.AreaConfiguration.ConfigParameters.DisappearTime = value;
        return this;
    }
    /// <summary>
    /// Sets the area IDs for the report query.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ReportQueryBatchProcessCreationBuilder WithAreaIds(List<int> value)
    {
        _query.AreaConfiguration.AreaIds = value;
        return this;
    }
    public ReportQueryBatchProcessCreationBuilder WithIntegrationKeys(List<string> value)
    {
        _query.IntegrationKeys = value;
        return this;
    }
    /// <summary>
    /// Sets the start local time for the report query.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ReportQueryBatchProcessCreationBuilder WithStartLocalTime(DateTime value)
    {
        _query.TimeParameters.Start = value;
        return this;
    }
/// <summary>
/// Sets the end local time for the report query.
/// </summary>
/// <param name="value"></param>
/// <returns></returns>
    public ReportQueryBatchProcessCreationBuilder WithEndLocalTime(DateTime value)
    {
        _query.TimeParameters.End = value;
        return this;
    }
    /// <summary>
    /// Adds a webhook URL with basic authentication headers to the report query.
    /// </summary>
    /// <param name="webhookUrl"></param>
    /// <param name="webhookUserName"></param>
    /// <param name="webhookPassword"></param>
    /// <returns></returns>
    public ReportQueryBatchProcessCreationBuilder WithWebHookUrl(string webhookUrl, string webhookUserName, string webhookPassword)
    {
        _query.AdditionalInformation.WebHooks.Add(new ReportWebHook
        {
            Url = webhookUrl,
            View = ReportContentView.GLOBAL.ToString(),
            Headers = new Dictionary<string, string>
            {
                { "Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{webhookUserName}:{webhookPassword}"))}" }
            }
        });
        return this;
    }
    /// <summary>
    /// Builds the report request with all the set parameters.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public ReportRequest Build()
    {
        var missingRequiredProps = _query.GetType().GetProperties()
            .Where(p => Attribute.IsDefined(p, typeof(RequiredAttribute)) && p.GetValue(_query) == null)
            .Select(p => p.Name).ToList();

        if (missingRequiredProps.Any())
        {
            var props = string.Join(", ", missingRequiredProps);
            throw new Exception($"The following properties are marked as Required but have null values: {props}.");
        }

        return _query;
    }
}
