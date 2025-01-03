using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;

public class InMemoryEmailRepository : IInMemoryEmailRepository
{
    private readonly ConcurrentDictionary<string, Email> _emailList = new();
    private readonly ILogger<InMemoryEmailRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;
    private readonly string fileName = "Email.json";
    public InMemoryEmailRepository(ILogger<InMemoryEmailRepository> logger, IConfiguration configuration, IFileService fileService)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        // Load Connection data from the first file into the first collection
       LoadDataFromFile().Wait();
    }

    public async Task<Email?> Add(Email email)
    {
        //add to email and also save to file
        bool saveToFile = false;
        try
        {
            if (_emailList.TryAdd(email.Id, email))
            {
                saveToFile = true;
                return email;

            }
            else
            {
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_emailList.Values, Formatting.Indented));
            }
        }
    }

    public async Task<Email?> Delete(string id)
    {
        bool saveToFile = false;
        try
        {
            //delete from email and also save to file
            if (_emailList.TryRemove(id, out Email currentEmail))
            {
                saveToFile = true;
                return currentEmail;

            }
            else
            {
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_emailList.Values, Formatting.Indented));
            }
        }
    }
    public async Task<Email?> Update(string id, Email email)
    {
        bool saveToFile = false;
        try
        {
            if (_emailList.TryGetValue(id, out Email? currentEmail) && _emailList.TryUpdate(id, email, currentEmail))
            {
                saveToFile = true;
                return currentEmail;
            }
            else
            {
                return null;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
        finally
        {
            if (saveToFile)
            {
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_emailList.Values, Formatting.Indented));
            }
        }
    }

    public IEnumerable<Email> GetAll()
    {
        //return all emails
        return _emailList.Values;
    }
    /// <summary>
    /// Updates a email in the in-memory email repository.
    /// </summary>
    private async Task LoadDataFromFile()
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(fileName);
            if (!string.IsNullOrEmpty(fileContent))
            {
                // Parse the file content to get the data. This depends on the format of your file.
                // Here's an example if your file was in JSON format and contained an array of T objects:
                List<Email>? data = JsonConvert.DeserializeObject<List<Email>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data != null && data.Count != 0)
                {
                    foreach (Email item in data)
                    {
                        _emailList.TryAdd(item.Id, item);
                    }
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

    public Task<bool> ResetEmailsList()
    {
        try
        {
            _emailList.Clear();
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult(true);
        }
    }

    public Task<bool> SetupEmailsList()
    {
        try
        {
            // Load data from the first file into the first collection
            LoadDataFromFile().Wait();

            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult(true);
        }
    }
}