

public interface IFilePathProvider
{
    Task<string> GetConfigurationDirectory();
    Task<string> GetLogDirectory();
    Task<string> GetBasePath(string directory);
}