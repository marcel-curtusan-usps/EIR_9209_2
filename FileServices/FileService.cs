using System.Text;
/// <summary>
/// Provides file-related services.
/// </summary>

/// <summary>
/// Interface for file services, including reading and writing files.
/// </summary>
public class FileService : IFileService
{
    private const string FileWriteErrorMessage = "An error occurred when writing the file: {Message}";
    private const string FileNotFoundErrorMessage = "File not found: {FileName}";
    private const string UnexpectedErrorMessage = "An unexpected error occurred: {Message}";
    /// <summary>
    /// Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    private readonly ILogger _logger;
    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    private readonly IConfiguration _configuration;
    /// <summary>
    /// Gets the configuration instance.
    /// </summary>
    private readonly IFilePathProvider _filePath;
    /// <summary>
    /// Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="configuration"></param>
    /// <param name="filePath"></param>
    public FileService(ILogger<FileService> logger, IConfiguration configuration, IFilePathProvider filePath)
    {
        _logger = logger;
        _configuration = configuration;
        _filePath = filePath;
    }
    /// <summary>
    /// Reads the content of a file asynchronously.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
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
                    _logger.LogError("File not found: {PathWithFileName}", PathWithFileName);
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
            _logger.LogError(ex, FileNotFoundErrorMessage, ex.FileName);
            return string.Empty;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, FileWriteErrorMessage, ex.Message);
            return string.Empty;
        }
        catch (Exception e)
        {
            _logger.LogError(e, UnexpectedErrorMessage, e.Message);
            return string.Empty;
        }

    }
  /// <summary>
  /// Writes the specified content to a configuration file with the given name.
  /// </summary>
  /// <param name="fileName"></param>
  /// <param name="content"></param>
  /// <returns></returns>
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
            _logger.LogError(ex, FileNotFoundErrorMessage, ex.FileName);
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError(ex, FileWriteErrorMessage, ex.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, UnexpectedErrorMessage, e.Message);
        }
    }
    /// <summary>
    /// Writes the specified content to a log file with the given name.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="content"></param>
    /// <returns></returns>
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
            _logger.LogError(ex, FileNotFoundErrorMessage, ex.FileName);
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError(ex, FileWriteErrorMessage, ex.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, UnexpectedErrorMessage, e.Message);
        }
    }
    /// <summary>
    /// Reads the content of a file from the specified root directory.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="directory"></param>
    /// <returns></returns>
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
                    _logger.LogError(FileNotFoundErrorMessage, PathWithFileName);
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
            _logger.LogError(ex, FileNotFoundErrorMessage, ex.FileName);
            return string.Empty;
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError(ex, FileWriteErrorMessage, ex.Message);
            return string.Empty;
        }
        catch (Exception e)
        {
            _logger.LogError(e, UnexpectedErrorMessage, e.Message);
            return string.Empty;
        }

    }
    /// <summary>
    /// Writes the specified content to a file with the given name in the specified root directory.
    /// </summary>
    /// <param name="fileName">The name of the file to write.</param>
    /// <param name="directory">The root directory where the file will be written.</param>
    /// <param name="content">The content to write to the file.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
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

                await sr.WriteLineAsync(content);
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            _logger.LogError(ex, FileNotFoundErrorMessage, ex.FileName);
            // You can choose to throw an exception or take any other appropriate action
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            _logger.LogError(ex, FileWriteErrorMessage, ex.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, UnexpectedErrorMessage, e.Message);
        }

    }
}