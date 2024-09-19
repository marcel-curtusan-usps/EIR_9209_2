public interface IFilePathProvider
{
    Task<string> GetFilePath();
    Task<string> GetBasePath(string directory);
}