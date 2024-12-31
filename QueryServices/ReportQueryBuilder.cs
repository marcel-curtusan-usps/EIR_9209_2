using System.ComponentModel.DataAnnotations;

public class ReportQueryBuilder
{
    private readonly ReportQuery _query = new ReportQuery();
    public ReportQueryBuilder WithSearch(string value)
    {
        _query.Search = value;
        return this;
    }
    public ReportQueryBuilder WithOrder(string value)
    {
        _query.Order = value;
        return this;
    }
    public ReportQueryBuilder WithIsDesc(string value)
    {
        _query.Desc = value;
        return this;
    }

    public ReportQueryBuilder WithTagIds(List<string> value)
    {
        _query.TagIds = value;
        return this;
    }

    public ReportQueryBuilder WithGroupIds(List<string> value)
    {
        _query.GroupIds = value;
        return this;
    }

    public ReportQueryBuilder WithMinTimeOnArea(TimeSpan value)
    {
        _query.MinTimeOnArea = value;
        return this;
    }

    public ReportQueryBuilder WithTimeStep(TimeSpan value)
    {
        _query.TimeStep = value;
        return this;
    }
    public ReportQueryBuilder WithActivationTime(TimeSpan value)
    {
        _query.ActivationTime = value;
        return this;
    }
    public ReportQueryBuilder WithDeactivationTime(TimeSpan value)
    {
        _query.DeactivationTime = value;
        return this;
    }
    public ReportQueryBuilder WithDisappearTime(TimeSpan value)
    {
        _query.DisappearTime = value;
        return this;
    }
    public ReportQueryBuilder WithAreaIds(List<string> value)
    {
        _query.AreaIds = value;
        return this;
    }

    public ReportQueryBuilder WithAreaGroupIds(List<string> value)
    {
        _query.AreaGroupIds = value;
        return this;
    }

    public ReportQueryBuilder WithStartLocalTime(DateTime value)
    {
        _query.StartTime = value;
        return this;
    }

    public ReportQueryBuilder WithEndLocalTime(DateTime value)
    {
        _query.EndTime = value;
        return this;
    }

    public ReportQueryBuilder WithQueryType(ESelsReportQueryType value)
    {
        _query.Type = value;
        return this;
    }

    public ReportQuery Build()
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
