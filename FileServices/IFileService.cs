// Ignore Spelling: Mongo

public interface IFileService
{
    Task<string> ReadFile(string path);

    Task WriteConfigurationFile(string fileName, string content);
    Task<string> ReadFileFromRoot(string fileName, string directory);
    Task WriteFileInRoot(string path, string directory, string content);
    Task WriteLogFile(string fileName, string content);
}