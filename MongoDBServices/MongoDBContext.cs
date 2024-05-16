// Ignore Spelling: Mongo

using EIR_9209_2.Models;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Driver;
using Newtonsoft.Json;

public class MongoDBContext
{
    internal static MongoDbRunner? _runner;
    public IMongoDatabase Database { get; }
    private IFileService FileService { get; }
    private IMongoClient mongoClient { get; }

    public MongoDBContext(IOptions<MongoDBSettings> settings, IConfiguration configuration, IFileService fileService)
    {

        // Start the MongoDB Memory Server
        if (configuration[key: "MongoDB:RemoteDBConnection"] == false.ToString())
        {
            FileService = fileService;
            _runner = MongoDbRunner.Start(singleNodeReplSet: false);
            mongoClient = new MongoClient(_runner.ConnectionString);
            Database = mongoClient.GetDatabase(configuration[key: "MongoDB:DatabaseName"]) as IMongoDatabase;

            string BuildConnectionPath = Path.Combine(configuration[key: "ApplicationConfiguration:BaseDrive"], configuration[key: "ApplicationConfiguration:BaseDirectory"], configuration[key: "SiteIdentity:NassCode"], configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{configuration[key: "MongoDB:CollectionConnections"]}.json");
            // Load data from the first file into the first collection
            Task.Run(async () => await LoadDataFromFile<Connection>(BuildConnectionPath, ConnectionList));

            string BuildBackgroundImagePath = Path.Combine(configuration[key: "ApplicationConfiguration:BaseDrive"], configuration[key: "ApplicationConfiguration:BaseDirectory"], configuration[key: "SiteIdentity:NassCode"], configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{configuration[key: "MongoDB:CollectionBackgroundImages"]}.json");
            // Load data from the second file into the second collection
            Task.Run(async () => await LoadDataFromFile<BackgroundImage>(BuildBackgroundImagePath, BackgroundImages));
        }
        else
        {
            mongoClient = new MongoClient(settings.Value.ConnectionString);
            Database = mongoClient.GetDatabase(configuration[key: "MongoDB:DatabaseName"]) as IMongoDatabase;
        }


    }
    private async Task LoadDataFromFile<T>(string filePath, IMongoCollection<T> collection)
    {
        try
        {
            // Read data from file
            var fileContent = await FileService.ReadFile(filePath);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            var data = JsonConvert.DeserializeObject<List<T>>(fileContent);

            // Insert the data into the MongoDB collection
            collection.InsertMany(data);
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
        catch (MongoException ex)
        {
            // Handle errors when inserting data into MongoDB
            Console.WriteLine($"An error occurred when inserting data into MongoDB: {ex.Message}");
        }
    }
    public IMongoCollection<Connection> ConnectionList
    {
        get
        {
            return Database.GetCollection<Connection>("connectionList");
        }
    }
    public IMongoCollection<BackgroundImage> BackgroundImages
    {
        get
        {
            return Database.GetCollection<BackgroundImage>("backgroundImages");
        }
    }
}
