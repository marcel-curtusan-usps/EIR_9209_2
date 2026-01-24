using System;
using Newtonsoft.Json.Linq;

namespace EIR_9209_2.DatabaseCalls.MPE;

/// <summary>
/// MPE Interface
/// </summary>
public interface IMpe
{
    /// <summary>
    /// Get MPE Data
    /// </summary>
    /// <param name="data"></param>
    Task<(object?, object?)> GetMpeData(JToken data);
}
