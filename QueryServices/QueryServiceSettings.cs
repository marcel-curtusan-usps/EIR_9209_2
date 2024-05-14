
public class QueryServiceSettings
{
    public string BaseQueryUrlWithPort { get; }
    public string FullUrl { get; }
    private string getBaseURL(Uri baseUrl)
    {
        UriBuilder builder = new UriBuilder(baseUrl);
        return builder.Uri.GetLeftPart(UriPartial.Authority);
    }
    public QueryServiceSettings(Uri baseUrl)
    {
        BaseQueryUrlWithPort = getBaseURL(baseUrl);
        FullUrl = baseUrl.ToString();
    }
}