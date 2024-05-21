using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace EIR_9209_2.InMemory
{
    public class InMemoryTagsRepository : IInMemoryTagsRepository
    {
        private static readonly ConcurrentDictionary<string, GeoMarker> _tagList = new();
        private readonly ILogger<InMemoryTagsRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileService FileService;
        public InMemoryTagsRepository(ILogger<InMemoryTagsRepository> logger, IConfiguration configuration, IFileService fileService)
        {
            FileService = fileService;
            _logger = logger;
            _configuration = configuration;
            string BuildPath = Path.Combine(configuration[key: "ApplicationConfiguration:BaseDrive"], configuration[key: "ApplicationConfiguration:BaseDirectory"], configuration[key: "SiteIdentity:NassCode"], configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{configuration[key: "MongoDB:Tags"]}.json");
            // Load data from the first file into the first collection
            _ = LoadDataFromFile(BuildPath);

        }
        public void Add(GeoMarker tag)
        {
            _tagList.TryAdd(tag.Properties.Id, tag);
        }

        public void Remove(string connectionId)
        {
            _tagList.TryRemove(connectionId, out _);
        }

        public GeoMarker Get(string id)
        {
            _tagList.TryGetValue(id, out GeoMarker tag);
            return tag;
        }

        public List<GeoMarker> GetAll()
        {
            return _tagList.Values.Where(r => r.Properties.posAge > 1 && r.Properties.posAge < 100000 && r.Properties.Visible).Select(y => y).ToList();
        }

        public void Update(GeoMarker tag)
        {
            if (_tagList.TryGetValue(tag.Properties.Id, out GeoMarker currentTag))
            {
                _tagList.TryUpdate(tag.Properties.Id, tag, currentTag);
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
                List<GeoMarker> data = JsonConvert.DeserializeObject<List<GeoMarker>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data.Count != 0)
                {
                    foreach (GeoMarker item in data.Select(r => r).ToList())
                    {
                        _tagList.TryAdd(item.Properties.Id, item);
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
}
