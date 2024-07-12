using Serilog;


// read configuration from appsettings.json
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

//configure Serilog using the configuration
Log.Logger = new LoggerConfiguration()
     .WriteTo.Console()
    .ReadFrom.Configuration(config)
    .CreateLogger();
try
{
    Log.Information("Starting up");
    CreateWebHostBuilder(args).Build().Run();
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
