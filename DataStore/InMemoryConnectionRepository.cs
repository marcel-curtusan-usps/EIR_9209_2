using EIR_9209_2.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

public class InMemoryConnectionRepository : IInMemoryConnectionRepository
{
    public readonly ConcurrentDictionary<string, Connection> _connectionList = new();
    private readonly ILogger<InMemoryConnectionRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileService FileService;

    public InMemoryConnectionRepository(ILogger<InMemoryConnectionRepository> logger, IConfiguration configuration, IFileService fileService)
    {
        FileService = fileService;
        string BuildConnectionPath = Path.Combine(configuration[key: "ApplicationConfiguration:BaseDrive"], configuration[key: "ApplicationConfiguration:BaseDirectory"], configuration[key: "SiteIdentity:NassCode"], configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{configuration[key: "MongoDB:CollectionConnections"]}.json");
        // Load data from the first file into the first collection
        Task.Run(async () => await LoadDataFromFile(BuildConnectionPath));

    }
    public void Add(Connection connection)
    {
        _connectionList.TryAdd(connection.Id, connection);
    }
    public void Remove(string connectionId)
    {
        _connectionList.TryRemove(connectionId, out _);
    }

    public Connection Get(string id)
    {
        _connectionList.TryGetValue(id, out Connection connection);
        return connection;
    }

    public IEnumerable<Connection> GetAll()
    {
        return _connectionList.Values;
    }
    public void Update(Connection connection)
    {
        if (_connectionList.TryGetValue(connection.Id, out Connection currentConnection))
        {
            _connectionList.TryUpdate(connection.Id, connection, currentConnection);
        }
    }
    private async Task LoadDataFromFile(string filePath)
    {
        try
        {
            // Read data from file
            var fileContent = await FileService.ReadFile(filePath);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            List<Connection> data = JsonConvert.DeserializeObject<List<Connection>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data.Any())
            {
                foreach (Connection item in data.Select(r => r).ToList())
                {
                    Add(item);
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            // Handle the FileNotFoundException here
            Console.WriteLine($"File not found: {ex.FileName}");
            // You can choose to throw an exception or take any other appropriate action
        }
        catch (IOException ex)
        {
            // Handle errors when reading the file
            Console.WriteLine($"An error occurred when reading the file: {ex.Message}");
        }
        catch (JsonException ex)
        {
            // Handle errors when parsing the JSON
            Console.WriteLine($"An error occurred when parsing the JSON: {ex.Message}");
        }
    }


}