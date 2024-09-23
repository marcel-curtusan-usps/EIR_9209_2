public class FilePathProvider : IFilePathProvider
{
    private readonly IConfiguration _configuration;

    public FilePathProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> GetFilePath()
    {
        string baseDrive = _configuration["ApplicationConfiguration:BaseDrive"] ?? throw new ArgumentNullException("BaseDrive");
        string baseDirectory = _configuration["ApplicationConfiguration:BaseDirectory"] ?? throw new ArgumentNullException("BaseDirectory");
        string nassCode = _configuration["ApplicationConfiguration:NassCode"] ?? throw new ArgumentNullException("NassCode");
        string configurationDirectory = _configuration["ApplicationConfiguration:ConfigurationDirectory"] ?? throw new ArgumentNullException("ConfigurationDirectory");

        string filePath = Path.Combine(baseDrive, baseDirectory, nassCode, configurationDirectory);
        if (string.IsNullOrEmpty(nassCode))
        {
            return "";
        }
        return filePath;
    }

    public async Task<string> GetBasePath(string directory)
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), directory);
        return filePath;
    }
}
   