
public class QueryServiceSettings
{
    public string BaseQueryUrlWithPort { get; }
    public string FullUrl { get; }
    public TimeSpan Timeout { get; } // Add Timeout property

    private string getBaseURL(Uri baseUrl)
    {
        UriBuilder builder = new UriBuilder(baseUrl);
        return builder.Uri.GetLeftPart(UriPartial.Authority);
    }

    public QueryServiceSettings(Uri baseUrl, TimeSpan timeout)
    {
        BaseQueryUrlWithPort = getBaseURL(baseUrl);
        FullUrl = baseUrl.ToString();
        Timeout = timeout; // Initialize Timeout property
    }
}
