// Ignore Spelling: Mongo

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
    public async Task WriteFileAsync(string fileName, string content)
    {
        try
        {
            string? baseDrive = _configuration["ApplicationConfiguration:BaseDrive"];
            string? baseDirectory = _configuration["ApplicationConfiguration:BaseDirectory"];
            string? siteId = _configuration["ApplicationConfiguration:NassCode"];
            string? configurationDirectory = _configuration["ApplicationConfiguration:ConfigurationDirectory"];

            if (!string.IsNullOrEmpty(baseDrive) && !string.IsNullOrEmpty(siteId))
            {
                var buildPath = Path.Combine(baseDrive, baseDirectory, siteId, configurationDirectory);
                var buildPathWithFileName = Path.Combine(buildPath, fileName);

                // Ensure the directory exists
                Directory.CreateDirectory(buildPath);

                using var file = new FileStream(buildPathWithFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                using StreamWriter sr = new(file, Encoding.UTF8);

                await sr.WriteLineAsync(content);
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when writing the file: {ex.Message}");
        }
        catch (Exception e)
        {
            _logger.LogError($"An unexpected error occurred: {e.Message}");
        }
    }

    public async Task WriteFileInAppConfig(string fileName, string content)
    {
        try
        {
            string? baseDrive = _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"];

            if (!string.IsNullOrEmpty(baseDrive))
            {
                var BuildPath = Path.Combine(Directory.GetCurrentDirectory(), baseDrive);

                var BuildPathWithFileName = Path.Combine(BuildPath, fileName);
                using var file = new FileStream(BuildPathWithFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                using StreamWriter sr = new(file, Encoding.UTF8);

                sr.WriteLine(content);
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when reading the file: {ex.Message}");
        }
        catch (Exception e)
        {
            _logger.LogError($"error: {e.Message}");
        }

    }
}