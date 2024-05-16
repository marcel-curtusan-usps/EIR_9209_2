// Ignore Spelling: Mongo

public class FileService : IFileService
{
    public string ReadFile(string path)
    {
        return File.ReadAllText(path);
    }

    public void WriteFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }
}