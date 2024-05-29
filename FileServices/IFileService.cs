// Ignore Spelling: Mongo

public interface IFileService
{
    Task<string> ReadFile(string path);
    bool WriteFile(string path, string content);
}