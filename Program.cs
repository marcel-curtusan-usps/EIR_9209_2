using EIR_9209_2.Utilities;
using Serilog;


//SETUP LOGGER
if (!ConfigureLogger.TryConfigureSerilog(out var failureMessage))
{
    SerilogDefaultLogger.LogError(failureMessage);
    DefaultResponseEndpoint.Start(failureMessage);
    Console.WriteLine($"An error occurred: {failureMessage}");
    return;
}
Log.Information("Starting up");
try
{
    CreateWebHostBuilder(args).Build().Run();
    Log.Information("Startup complete");
}
catch (Exception ex)
{

    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();

}
/// <summary>
/// Create the web host builder.
/// </summary>
/// <param name="args"></param>
/// <returns>IWebHostBuilder</returns>
static IHostBuilder CreateWebHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
    });
