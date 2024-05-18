using EIR_9209_2.Models;
using Newtonsoft.Json.Linq;

public interface IQueryService
{
    Task<QuuppaTag> GetQuuppaTagData(CancellationToken token);
}