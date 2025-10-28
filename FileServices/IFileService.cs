
/// <summary>
/// Provides methods for reading and writing files, including configuration and log files,
/// as well as operations within specified root directories.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Reads the contents of a file at the specified path.
    /// </summary>
    /// <param name="path">The path to the file to read.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the file contents as a string.</returns>
    Task<string> ReadFile(string path);

    /// <summary>
    /// Writes the specified content to a configuration file with the given name.
    /// </summary>
    /// <param name="fileName">The name of the configuration file.</param>
    /// <param name="content">The content to write to the file.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task WriteConfigurationFile(string fileName, string content);

    /// <summary>
    /// Reads the contents of a file from the specified root directory.
    /// </summary>
    /// <param name="fileName">The name of the file to read.</param>
    /// <param name="directory">The root directory where the file is located.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the file contents as a string.</returns>
    Task<string> ReadFileFromRoot(string fileName, string directory);

    /// <summary>
    /// Writes the specified content to a file in the given root directory.
    /// </summary>
    /// <param name="path">The path to the file to write.</param>
    /// <param name="directory">The root directory where the file will be written.</param>
    /// <param name="content">The content to write to the file.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task WriteFileInRoot(string path, string directory, string content);

    /// <summary>
    /// Writes the specified content to a log file with the given name.
    /// </summary>
    /// <param name="fileName">The name of the log file.</param>
    /// <param name="content">The content to write to the log file.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task WriteLogFile(string fileName, string content);
}