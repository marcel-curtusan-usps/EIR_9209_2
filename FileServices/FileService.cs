// Ignore Spelling: Mongo

using EIR_9209_2.Utilities;
using System.Text;

public class FileService : IFileService
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileAccessTester _accessTester;
    public FileService(ILogger<FileService> logger, IConfiguration configuration, IFileAccessTester accessTester)
    {
        _logger = logger;
        _configuration = configuration;
        _accessTester = accessTester;

    }
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

    public void WriteFile(string fileName, string content)
    {
        string baseDrive = _configuration[key: "ApplicationConfiguration:BaseDrive"];
        string siteid = _configuration[key: "SiteIdentity:NassCode"];

        if (!string.IsNullOrEmpty(baseDrive) && !string.IsNullOrEmpty(siteid))
        {
            string BuildPath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"], _configuration[key: "ApplicationConfiguration:BaseDirectory"], siteid, _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{fileName}");
            if (_accessTester.CanCreateFilesAndWriteInFolder(BuildPath))
            {
                using (FileStream file = new FileStream(BuildPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (StreamWriter sr = new StreamWriter(file, Encoding.UTF8))
                {

                    sr.WriteLine(content);
                }
            }

        }
    }
}