// Ignore Spelling: Mongo

public interface IFileService
{
    Task<string> ReadFile(string path);
    Task WriteFileInAppConfig(string path, string content);
    Task WriteFileAsync(string fileName, string content);
}