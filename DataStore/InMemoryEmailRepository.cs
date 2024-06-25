using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;

public class InMemoryEmailRepository : IInMemoryEmailRepository
{
    private readonly ConcurrentDictionary<string, Email> _emailList = new();
    private readonly ILogger<InMemoryEmailRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;
    private readonly string filePath = "";
    private readonly string fileName = "";
    public InMemoryEmailRepository(ILogger<InMemoryEmailRepository> logger, IConfiguration configuration, IFileService fileService)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        fileName = $"{_configuration[key: "InMemoryCollection:CollectionEmail"]}.json";
        filePath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"],
            _configuration[key: "ApplicationConfiguration:BaseDirectory"],
            _configuration[key: "ApplicationConfiguration:NassCode"],
            _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
            $"{fileName}");
        // Load Connection data from the first file into the first collection
        _ = LoadDataFromFile(filePath);
    }

    public Email? Add(Email email)
    {
        //add to email and also save to file
       
        if (_emailList.TryAdd(email.Id, email))
        {
            if (_fileService.WriteFile(fileName, JsonConvert.SerializeObject(_emailList.Values, Formatting.Indented)))
            {
                return email;
            }
            else
            {
                _logger.LogError($"{fileName} was not update");
                return null;

            }
        }
        else
        {
            return null;
        }
    }

    public Email? Delete(string id)
    {
        //delete from email and also save to file
        if (_emailList.TryRemove(id, out Email currentEmail))
        {
            if (_fileService.WriteFile(fileName, JsonConvert.SerializeObject(_emailList.Values, Formatting.Indented)))
            {
                return currentEmail;
            }
            else
            {
                return null;
            }

        }
        else
        {
            return null;
        }
    }

    public IEnumerable<Email> GetAll()
    {
        //return all emails

        return _emailList.Values;
    }

    public Email? Update(Email email)
    {
        if (_emailList.TryGetValue(email.Id, out Email? currentEmail) && _emailList.TryUpdate(email.Id, email, currentEmail))
        {
            if (_fileService.WriteFile(fileName, JsonConvert.SerializeObject(_emailList.Values, Formatting.Indented)))
            {
                return currentEmail;
            }
            else
            {
                return null;
            }

        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Updates a email in the in-memory email repository.
    /// </summary>
    /// <param name="FilePath"></param>

    private async Task LoadDataFromFile(string FilePath)
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(FilePath);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            List<Email> data = JsonConvert.DeserializeObject<List<Email>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data != null && data.Count != 0)
            {
                foreach (Email item in data)
                {
                    _emailList.TryAdd(item.Id, item);
                }
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
        catch (JsonException ex)
        {
            // Handle errors when parsing the JSON
            _logger.LogError($"An error occurred when parsing the JSON: {ex.Message}");
        }
    }
}