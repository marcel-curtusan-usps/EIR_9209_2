

using System.ComponentModel.DataAnnotations;

public class ReportQueryIDSBuilder
{

    private readonly ReportIDSQuery _query = new ReportIDSQuery();

    public ReportQueryIDSBuilder WithQueryName(string value)
    {
        _query.QueryName = value;
        return this;
    }
    public ReportQueryIDSBuilder WithstartHour(int value)
    {
        _query.StartHour = value;
        return this;
    }
    internal ReportQueryIDSBuilder WithendHour(int value)
    {
        _query.EndHour = value;
        return this;
    }

    public ReportIDSQuery Build()
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