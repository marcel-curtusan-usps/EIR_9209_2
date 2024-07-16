
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

internal class SerilogDefaultLogger
{
    public static void LogError(string error)
    {
        var defaultLogLocation = Path.GetTempPath();
        var logger = new LoggerConfiguration()
            .WriteTo.File(
                path: $"{defaultLogLocation}{typeof(Program).Assembly.GetName().Name}_DefaultLogger_.txt",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
        logger.Error(error);
        logger.Dispose();
    }
}