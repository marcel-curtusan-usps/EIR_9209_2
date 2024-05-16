// Ignore Spelling: Mongo

public class FileService : IFileService
{
    public async Task<string> ReadFile(string path)
    {
        if (File.Exists(path))
        {
            // File exists, safe to read
            return await File.ReadAllTextAsync(path);
        }
        else
        {
            // File does not exist
            throw new FileNotFoundException("File does not exist: " + path);
        }
    }

    public void WriteFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }
}