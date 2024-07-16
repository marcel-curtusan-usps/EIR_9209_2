// Ignore Spelling: Mongo

using EIR_9209_2.Utilities;
using System.Text;

public class FileService : IFileService
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    public FileService(ILogger<FileService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

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

    public bool WriteFile(string fileName, string content)
    {
        try
        {
            string? baseDrive = _configuration[key: "ApplicationConfiguration:BaseDrive"];
            string? baseDirectory = _configuration[key: "ApplicationConfiguration:BaseDirectory"];
            string? siteId = _configuration[key: "ApplicationConfiguration:NassCode"];
            string? configurationDirectory = _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"];

            if (!string.IsNullOrEmpty(baseDrive) && !string.IsNullOrEmpty(siteId))
            {
                var BuildPath = Path.Combine(baseDrive, baseDirectory, siteId, configurationDirectory);
                var BuildPathWithFileName = Path.Combine(BuildPath, fileName);
                //if (_accessTester.CanCreateFilesAndWriteInFolder(BuildPath))
                //{
                using var file = new FileStream(BuildPathWithFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                using StreamWriter sr = new(file, Encoding.UTF8);

                sr.WriteLine(content);
                return true;
                //}

            }
            return false;
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
            return false;
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when reading the file: {ex.Message}");
            return false;
        }
        catch (Exception e)
        {

            _logger.LogError($"error: {e.Message}");
            return false;
        }

    }
    public bool WriteFileInAppConfig(string fileName, string content)
    {
        try
        {
            string? baseDrive = _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"];

            if (!string.IsNullOrEmpty(baseDrive))
            {
                var BuildPath = Path.Combine(Directory.GetCurrentDirectory(), baseDrive);

                var BuildPathWithFileName = Path.Combine(BuildPath, fileName);
                //   if (_accessTester.CanCreateFilesAndWriteInFolder(BuildPath))
                //  {
                using var file = new FileStream(BuildPathWithFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                using StreamWriter sr = new(file, Encoding.UTF8);

                sr.WriteLine(content);
                return true;
                //  }

            }
            return false;
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
            return false;
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when reading the file: {ex.Message}");
            return false;
        }
        catch (Exception e)
        {

            _logger.LogError($"error: {e.Message}");
            return false;
        }

    }
}