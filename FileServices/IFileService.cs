// Ignore Spelling: Mongo

public interface IFileService
{
    Task<string> ReadFile(string path);
    void WriteFile(string path, string content);
}