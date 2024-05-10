using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;

CreateWebHostBuilder(args).Build().Run();
/// <summary>
/// Create the web host builder.
/// </summary>
/// <param name="args"></param>
/// <returns>IWebHostBuilder</returns>
static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>();
