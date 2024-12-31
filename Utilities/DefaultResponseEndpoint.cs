
using Microsoft.AspNetCore.HostFiltering;
using Serilog;
using System.Net;

internal class DefaultResponseEndpoint
{
    public static void Start(string errorMessage)
    {
        try
        {
            var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions() { ContentRootPath = Directory.GetCurrentDirectory() });
            builder.Configuration.AddEnvironmentVariables();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.WebHost.UseKestrel((builderContext, options) =>
            {
                options.Configure(builderContext.Configuration.GetSection("Kestrel"), reloadOnChange: true);
            });
            builder.Services.PostConfigure<HostFilteringOptions>(options =>
            {
                options.AllowedHosts = ["*"];
            });
            builder.Services.AddRouting();
            builder.WebHost.UseIIS();
            builder.WebHost.UseIISIntegration();
            var app = builder.Build();
            app.UseRouting();
            app.Map("/{*slug}", async (HttpContext context, string slug) =>
            {
                Log.Information("Redirecting request for {RequestTarget} to error message", slug);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync($"The application failed to start normally.\nError:\n{errorMessage}");
            });
            app.Run();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "{ClassName} terminated unexpectedly", nameof(DefaultResponseEndpoint));
        }
    }
}