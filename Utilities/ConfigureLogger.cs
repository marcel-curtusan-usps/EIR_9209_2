using EIR_9209_2.Utilities.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace EIR_9209_2.Utilities
{
    public static class ConfigureLogger
    {

        public static bool TryConfigureSerilog(out string failureMessage)
        {
            failureMessage = string.Empty;
            IConfigurationRoot configuration;
            var logFilePath = Helper.GetLogFilePath();
            try
            {

#if DEBUG
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
                var envFile = $"appsettings.{env}.json";
                var basePath = Directory.GetCurrentDirectory();
                var envFilePath = Path.Combine(basePath, envFile);

                configuration = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(File.Exists(envFilePath) ? envFile : "appsettings.json", true, true)
                    .Build();
#else
                configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .Build();
#endif
            }
            catch (Exception e)
            {
                failureMessage = $"Serilog could not start because an exception was encountered during configuration load:{Environment.NewLine}{e}";
                return false;
            }

            try
            {
                if (!Directory.Exists(logFilePath))
                {
                    Directory.CreateDirectory(logFilePath);
                }
                var appHasPermissionToLogToSpecifiedFolder = FileAccessTester.CanCreateFilesAndWriteInFolder(logFilePath);
                if (!appHasPermissionToLogToSpecifiedFolder)
                {
                    failureMessage = $"Serilog could not start because the application does not have permissions to write to the specified log directory {logFilePath}";
                    return false;
                }
            }
            catch (Exception e)
            {
                failureMessage = $"Serilog could not start because an exception was encountered during the log permissions verification check:{Environment.NewLine}{e}";
            }
            try
            {
                
                Serilog.Debugging.SelfLog.Enable(Console.Error);
                var loggerConfiguration = new LoggerConfiguration()
                    .MinimumLevel.Is(Enum.Parse<LogEventLevel>(configuration.GetRequiredValue("Logger:MinimumLevel")))
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .Enrich.WithExceptionDetails();
                var formatterExpression =
                    "{@t:yyyy-MM-dd HH:mm:ss.fff} [{@l:u3}][{ThreadId}]{#if ClassName is not null and MemberName is not null} {ClassName}.{MemberName}{#end} {@m}\n{@x}";
                if (configuration.GetValue<bool>("Logger:LogToConsole"))
                {
                    loggerConfiguration.WriteTo.Console(formatter: new ExpressionTemplate(formatterExpression, theme: TemplateTheme.Literate));
                }

                if (!string.IsNullOrEmpty(logFilePath))
                {
                    loggerConfiguration.WriteTo.File(
                        path: Path.Combine(logFilePath, "logfile.txt"),
                        rollOnFileSizeLimit: true,
                        fileSizeLimitBytes: configuration.GetRequiredValue<long>("Logger:PerFileMaximumSizeInBytes"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileTimeLimit: TimeSpan.FromDays(configuration.GetRequiredValue<double>("Logger:FileRetentionMaximumDurationInDays")),
                        retainedFileCountLimit: (int)(configuration.GetRequiredValue<long>("Logger:FileRetentionMaximumTotalSizeInBytes") / configuration.GetRequiredValue<long>("Logger:PerFileMaximumSizeInBytes")),
                        formatter: new ExpressionTemplate(formatterExpression));
                }
                Log.Logger = loggerConfiguration.CreateLogger();
                return true;
            }
            catch (Exception e)
            {
                failureMessage = $"Serilog could not start because an exception was encountered during initialization:{Environment.NewLine}{e}";
                return false;
            }
        }
    }
}