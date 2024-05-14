using Newtonsoft.Json.Linq;

public interface IQueryService
{
    Task<JObject> GetData(CancellationToken token);
}