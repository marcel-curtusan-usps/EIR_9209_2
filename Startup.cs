using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using EIR_9209_2.Models;
using EIR_9209_2.SiteIdentity;
using EIR_9209_2.Utilities;
using EIR_9209_2.InMemory;
using EIR_9209_2.Service;

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
        AddOptions(services);
        services.AddSingleton<IFileService, FileService>();

        services.AddSingleton<IInMemoryConnectionRepository, InMemoryConnectionRepository>();
        services.AddSingleton<IInMemoryTagsRepository, InMemoryTagsRepository>();
        services.AddSingleton<IInMemoryGeoZonesRepository, InMemoryGeoZonesRepository>();
        services.AddSingleton<IInMemoryBackgroundImageRepository, InMemoryBackgroundImageRepository>();

        //add SignalR to the services
        services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 100_000;
                options.MaximumParallelInvocationsPerClient = 5;
            }).AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
            })
    .AddMessagePackProtocol();
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
            })
            .AddXmlSerializerFormatters();


        services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("1.0.0.1", new OpenApiInfo
                {
                    Version = "1.0.0.1",
                    Title = "Connected Facilities (CF)",
                    Description = "Swagger - OpenAPI 3.0",
                    Contact = new OpenApiContact()
                    {
                        Name = "Connected Facilities API Support",
                        Email = "cf-sels_support@usps.gov"
                    },

                });
                c.CustomSchemaIds(type => type.FullName);
                // c.IncludeXmlComments($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{_hostingEnv.ApplicationName}.xml");
                // Sets the basePath property in the Swagger document generated
                // c.DocumentFilter<BasePathFilter>("/api/v3");

                // Include DataAnnotation attributes on Controller Action parameters as Swagger validation rules (e.g required, pattern, ..)
                // Use [ValidateModelState] on Actions to actually validate it in C# as well!
                c.OperationFilter<GeneratePathParamsValidationFilter>();
            });
        services.AddSingleton<BackgroundWorkerService>();
        services.AddHostedService(provider => provider.GetRequiredService<BackgroundWorkerService>());
    }

    private void AddOptions(IServiceCollection services)
    {
        services.Configure<SiteIdentitySettings>(Configuration.GetSection("SiteIdentity"));
        try
        {
            var logFilePath = Path.Combine(Configuration[key: "ApplicationConfiguration:BaseDrive"], Configuration[key: "ApplicationConfiguration:BaseDirectory"], Configuration[key: "SiteIdentity:NassCode"]);
            if (!Directory.Exists(logFilePath))
            {
                Directory.CreateDirectory(logFilePath);
            }
            var appHasPermissionToLogToSpecifiedFolder = FileAccessTester.CanCreateFilesAndWriteInFolder(logFilePath);
            if (!appHasPermissionToLogToSpecifiedFolder)
            {
            }
        }
        catch (Exception e)
        {

        }
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
        //TODO: Uncomment this if you need wwwroot folder
        // app.UseStaticFiles();
        //app.UseHealthChecks("/health");
        app.UseAuthorization();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            //TODO: Either use the SwaggerGen generated Swagger contract (generated from C# classes)
            c.SwaggerEndpoint("/swagger/1.0.0.1/swagger.json", "Swagger");

            //TODO: Or alternatively use the original Swagger contract that's included in the static files
            // c.SwaggerEndpoint("/swagger-original.json", "Swagger  - OpenAPI 3.0 Original");
        });

        //TODO: Use Https Redirection
        // app.UseHttpsRedirection();
        app.UseFileServer();
        app.UseDefaultFiles();
        app.UseStaticFiles();
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