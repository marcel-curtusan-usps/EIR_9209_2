using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using EIR_9209_2.Models;
using EIR_9209_2.Controllers;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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

        //setup mongodb 
        services.Configure<MongoDBSettings>(Configuration.GetSection("MongoDB"));
        services.AddSingleton<MongoDBContext>();
        services.AddSingleton(provider => provider.GetRequiredService<MongoDBContext>().Database);
        services.AddSingleton<MongoDbHealthCheck>();
        services.AddHealthChecks().AddCheck<MongoDbHealthCheck>("mongodb");

        services.AddSingleton<IBackgroundImageRepository, BackgroundImageRepository>();
        services.AddSingleton<IConnectionRepository, ConnectionRepository>();
        services.AddSingleton<BackgroundServiceManager>();
        //Read configuration data from ConnectionList.json file from the Configuration folder
        //var connectionLists = new ConfigurationBuilder()
        //    .SetBasePath(Directory.GetCurrentDirectory())
        //    .AddJsonFile("Configuration/ConnectionList.json", optional: false, reloadOnChange: true)
        //    .Build();
        //var connectionList = connectionLists.GetSection("ConnectionList").Get<Connection[]>();
        //if (connectionList != null)
        //{
        //    foreach (var conn in connectionList)
        //    {
        //        var ConnectionRepository = services.BuildServiceProvider().GetRequiredService<IConnectionRepository>();
        //        ConnectionRepository.Add(conn).Wait();
        //    }
        //}
        //load background images in memory
        // var backgroundImageRepository = new BackgroundImageRepository();
        // services.AddSingleton<IBackgroundImageRepository>(backgroundImageRepository);

        //Read configuration data from ConnectionList.json file from the Configuration folder
        //var backgroundImages = new ConfigurationBuilder()
        //    .SetBasePath(Directory.GetCurrentDirectory())
        //    .AddJsonFile("Configuration/BackgroundImage.json", optional: false, reloadOnChange: true)
        //    .Build();
        //var backgroundImage = backgroundImages.GetSection("BackgroundImages").Get<BackgroundImage[]>();
        //if (backgroundImage != null)
        //{
        //    foreach (var image in backgroundImage)
        //    {
        //        var backgroundImageRepository = services.BuildServiceProvider().GetRequiredService<IBackgroundImageRepository>();
        //        backgroundImageRepository.Add(image).Wait();
        //    }
        //}
        // Start a new service for each record in the connection list
        //var ConnectionRepository = services.BuildServiceProvider().GetRequiredService<IConnectionRepository>();
        //var connectionList = ConnectionRepository.GetAll().Result;
        //if (connectionList != null)
        //{
        //    foreach (var connection in connectionList)
        //    {
        //        services.AddHttpClient(connection.Name);
        //        services.AddHostedService(provider =>
        //        {
        //            HttpClient httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient(connection.Name);
        //            return new ConnectionBackgroundService(provider.GetRequiredService<ILogger<ConnectionBackgroundService>>(), httpClient, connection, provider.GetRequiredService<IHubContext<HubServices>>(), provider.GetRequiredService<BackgroundServiceManager>(), connection.Id);
        //        });

        //    }
        //}
        //add SignalR to the services
        services.AddSignalR().AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }); ;
        services.AddSingleton<HubServices, HubServices>();
        // Add framework services.
        services
            .AddMvc(options =>
            {
                options.InputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonInputFormatter>();
                options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonOutputFormatter>();
            })
            .AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                opts.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
            })
            .AddXmlSerializerFormatters();


        services
            .AddSwaggerGen(c =>
            {
                c.SwaggerDoc("1.0.11", new OpenApiInfo
                {
                    Version = "1.0.11",
                    Title = "Swagger - OpenAPI 3.0",
                    Description = "Swagger - OpenAPI 3.0 (ASP.NET Core 8.0)",
                    Contact = new OpenApiContact()
                    {
                        Name = "Swagger Codegen Contributors",
                        Url = new Uri("https://github.com/swagger-api/swagger-codegen"),
                        Email = "apiteam@swagger.io"
                    },
                    TermsOfService = new Uri("http://swagger.io/terms/")
                });
                c.CustomSchemaIds(type => type.FullName);
                // c.IncludeXmlComments($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{_hostingEnv.ApplicationName}.xml");
                // Sets the basePath property in the Swagger document generated
                c.DocumentFilter<BasePathFilter>("/api/v3");

                // Include DataAnnotation attributes on Controller Action parameters as Swagger validation rules (e.g required, pattern, ..)
                // Use [ValidateModelState] on Actions to actually validate it in C# as well!
                //c.OperationFilter<GeneratePathParamsValidationFilter>();
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

        //TODO: Uncomment this if you need wwwroot folder
        // app.UseStaticFiles();
        app.UseHealthChecks("/health");
        app.UseAuthorization();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            //TODO: Either use the SwaggerGen generated Swagger contract (generated from C# classes)
            c.SwaggerEndpoint("/swagger/1.0.11/swagger.json", "Swagger - OpenAPI 3.0");

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