// Ignore Spelling: Mongo

using System.IO;
using System.Text;

public class FileService : IFileService
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IFilePathProvider _filePath;
    public FileService(ILogger<FileService> logger, IConfiguration configuration, IFilePathProvider filePath)
    {
        _logger = logger;
        _configuration = configuration;
        _filePath = filePath;
    }
    public async Task<string> ReadFile(string fileName)
    {
        try
        {
            var directoryPathName = await _filePath.GetConfigurationDirectory();
            if (!string.IsNullOrEmpty(directoryPathName))
            {
                var PathWithFileName = Path.Combine(directoryPathName, fileName);
                if (File.Exists(PathWithFileName))
                {
                    // File exists, safe to read
                    return await File.ReadAllTextAsync(PathWithFileName);
                }
                else
                {
                    // File does not exist
                    _logger.LogError($"File not found: {PathWithFileName}");
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            return string.Empty;
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when writing the file: {ex.Message}");
            return string.Empty;
        }
        catch (Exception e)
        {
            _logger.LogError($"An unexpected error occurred: {e.Message}");
            return string.Empty;
        }

    }
  
    public async Task WriteConfigurationFile(string fileName, string content)
    {
        try
        {
          var  directoryPathName = await _filePath.GetConfigurationDirectory();

            if (!string.IsNullOrEmpty(directoryPathName))
            {
                var PathWithFileName = Path.Combine(directoryPathName, fileName);

                // Ensure the directory exists
                Directory.CreateDirectory(directoryPathName);

                using var file = new FileStream(PathWithFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using StreamWriter sr = new(file, Encoding.UTF8);

                await sr.WriteLineAsync(content);
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
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
    public async Task WriteLogFile(string fileName, string content)
    {
        try
        {
            var directoryPathName = await _filePath.GetLogDirectory();

            if (!string.IsNullOrEmpty(directoryPathName))
            {
                var PathWithFileName = Path.Combine(directoryPathName, fileName);

                // Ensure the directory exists
                Directory.CreateDirectory(directoryPathName);
                if (File.Exists(PathWithFileName))
                {
                    // File exists, append to it
                    using var file = new FileStream(PathWithFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    using StreamWriter sr = new(file, Encoding.UTF8);
                    await sr.WriteLineAsync($",{content}");
                }
                else
                {
                    // File does not exist, create and write to it
                    using var file = new FileStream(PathWithFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    using StreamWriter sr = new(file, Encoding.UTF8);
                    await sr.WriteLineAsync(content);
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
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
    public async Task<string> ReadFileFromRoot(string fileName, string directory)
    {
        try
        {
            var directoryPathName = await _filePath.GetBasePath(directory);
            if (!string.IsNullOrEmpty(directoryPathName))
            {
                var PathWithFileName = Path.Combine(directoryPathName, fileName);
                if (File.Exists(PathWithFileName))
                {
                    // File exists, safe to read
                    return await File.ReadAllTextAsync(PathWithFileName);
                }
                else
                {
                    // File does not exist
                    _logger.LogError($"File not found: {PathWithFileName}");
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError($"File not found: {ex.FileName}");
            return string.Empty;
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError($"An error occurred when writing the file: {ex.Message}");
            return string.Empty;
        }
        catch (Exception e)
        {
            _logger.LogError($"An unexpected error occurred: {e.Message}");
            return string.Empty;
        }

    }
    public async Task WriteFileInRoot(string fileName, string directory, string content)
    {
        try
        {
          
            var directoryPathName = await _filePath.GetBasePath(directory);
            if (!string.IsNullOrEmpty(directoryPathName))
            {
                var BuildPathWithFileName = Path.Combine(directoryPathName, fileName);
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