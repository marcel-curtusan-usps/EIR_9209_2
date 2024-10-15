using EIR_9209_2.DatabaseCalls.IDS;
using EIR_9209_2.DataStore;
using EIR_9209_2.Service;
using EIR_9209_2.Utilities;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Text;

public class Startup
{
    private readonly IWebHostEnvironment _hostingEnv;
    private IConfiguration Configuration { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="env"></param>
    /// <param name="configuration"></param>
    public Startup(IWebHostEnvironment env, IConfiguration configuration)
    {
        _hostingEnv = env;
        Configuration = configuration;
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        // Configure logging
        services.AddLogging();
        services.AddAuthentication(IISDefaults.AuthenticationScheme); // Add Windows Authentication
        services.AddSingleton<IFilePathProvider, FilePathProvider>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<IResetApplication, ResetApplication>();
        services.AddSingleton<IInMemoryApplicationRepository, InMemoryApplicationRepository>();
        services.AddSingleton<IEncryptDecrypt, EncryptDecrypt>();
        services.AddSingleton<IInMemorySiteInfoRepository, InMemorySiteInfoRepository>();
        services.AddSingleton<IInMemoryTACSReports, InMemoryTACSReports>();
        services.AddSingleton<IInMemoryEmailRepository, InMemoryEmailRepository>();
        services.AddSingleton<IInMemoryBackgroundImageRepository, InMemoryBackgroundImageRepository>();
        services.AddSingleton<IInMemoryConnectionRepository, InMemoryConnectionRepository>();
        services.AddSingleton<IInMemoryDacodeRepository, InMemoryDacodeRepository>();
        services.AddSingleton<IInMemoryTagsRepository, InMemoryTagsRepository>();
        services.AddSingleton<IInMemoryGeoZonesRepository, InMemoryGeoZonesRepository>();
        services.AddSingleton<IInMemoryEmployeesRepository, InMemoryEmployeesRepository>();
        services.AddSingleton<IInMemoryCamerasRepository, InMemoryCamerasRepository>();
        services.AddSingleton<IIDS, IDS>();
        services.AddSingleton<ScreenshotService>();
        services.AddSingleton<EmailService>();
        services.AddSingleton<Worker>();
        services.AddHostedService(p => p.GetRequiredService<Worker>());
        services.AddHttpClient();
        //add SignalR to the services
        services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 100_000;
                options.MaximumParallelInvocationsPerClient = 5;
            }).AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
            }).AddMessagePackProtocol();
        services.AddCors(o =>
        {
            o.AddPolicy("Everything", p =>
            {
                p.AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowAnyOrigin();
            });
        });
   
        services.AddSingleton<HubServices>();
        // Add framework services.
        services.AddMvc(options =>
            {
                options.InputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonInputFormatter>();
                options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonOutputFormatter>();
            }).AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                opts.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
            }).AddXmlSerializerFormatters();
        services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "1.0.0.1",
                    Title = $"{Helper.GetAppName()}",
                    Description = "Swagger - OpenAPI 3.0",
                    Contact = new OpenApiContact()
                    {
                        Name = $"{Helper.GetAppName()} API Support",
                        Email = "cf-sels_support@usps.gov"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "USPS EMS Group License"
                    }

                });
                // using System.Reflection;
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                options.UseInlineDefinitionsForEnums();
                options.CustomSchemaIds(type => type.FullName);
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                options.OperationFilter<GeneratePathParamsValidationFilter>();
            });       

    }


    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    /// <param name="loggerFactory"></param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseRouting();
        app.UseAuthentication(); // Ensure authentication middleware is added
        app.UseAuthorization(); // Ensure authorization middleware is added
        var swaggerConfig = Configuration.GetSection("Swagger");
        var applicationConfiguration = Configuration.GetSection("ApplicationConfiguration");
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            if (env.IsDevelopment())
            {
                c.SwaggerEndpoint(swaggerConfig["Endpoint"], $"{applicationConfiguration["ApplicationName"]} API's");
            }
            else
            {
                c.SwaggerEndpoint($"/{applicationConfiguration["ApplicationName"]}{swaggerConfig["Endpoint"]}", $"{applicationConfiguration["ApplicationName"]} API's");
            }
        });
        app.UseFileServer(); 
        app.UseDefaultFiles(); // Use the configured options
        app.UseStaticFiles(); // Serve static files
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<HubServices>("/hubServics");
        });
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            //TODO: Enable production exception handling (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
    }
}