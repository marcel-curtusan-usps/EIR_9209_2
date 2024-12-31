using System.ComponentModel.DataAnnotations;


public class ReportQueryHCESBuilder()
{
    private readonly ReportHCESQuery _query = new ReportHCESQuery();

    public ReportQueryHCESBuilder WithAppId(string value)
    {
        _query.Appid = value;
        return this;
    }
    public ReportQueryHCESBuilder WithPageSize(int value)
    {
        _query.Pagesize = value;
        return this;
    }
    public ReportQueryHCESBuilder WithPageNumber(int value)
    {
        _query.Pagenumber = value;
        return this;
    }
    public ReportQueryHCESBuilder WithFilter(string fieldname,string value)
    {
       ;
        _query.Filter.Add( new()
        {
            query = new Query
            {
                fieldname = fieldname,
                operand = "=",
                fieldvalue = value.ToString()
            }
        });
        return this;
    }
    public ReportHCESQuery Build()
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

