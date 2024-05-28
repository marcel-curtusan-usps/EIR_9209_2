using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Security.Policy;

namespace EIR_9209_2.DataStore
{
    public class InMemorySiteInfoRepository : IInMemorySiteInfoRepository
    {
        private readonly static ConcurrentDictionary<string, SiteInformation> _siteInfo = new();
        private readonly ILogger<InMemorySiteInfoRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileService FileService;
        public InMemorySiteInfoRepository(ILogger<InMemorySiteInfoRepository> logger, IConfiguration configuration, IFileService fileService)
        {
            FileService = fileService;
            _logger = logger;
            _configuration = configuration;
            string BuildPath = Path.Combine(configuration[key: "ApplicationConfiguration:BaseDrive"], configuration[key: "ApplicationConfiguration:BaseDirectory"], configuration[key: "SiteIdentity:NassCode"], configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{configuration[key: "InMemoryCollection:CollectionSiteInformation"]}.json");
            // Load data from the first file into the first collection
            _ = LoadDataFromFile(BuildPath);

        }
        public void Add(SiteInformation site)
        {
            _siteInfo.TryAdd(site.FdbId, site);
        }

        public void Remove(string id)
        {
            _siteInfo.TryRemove(id, out _);
        }

        public SiteInformation Get(string id)
        {
            _siteInfo.TryGetValue(id, out SiteInformation tag);
            return tag;
        }

        public SiteInformation GetByNASSCode(string id)
        {
            return _siteInfo.Where(t => t.Value.SiteId == id).Select(y => y.Value).FirstOrDefault();
        }

        public List<SiteInformation> GetAll()
        {
            return _siteInfo.Values.Select(y => y).ToList();
        }

        public void Update(SiteInformation site)
        {
            if (_siteInfo.TryGetValue(site.FdbId, out SiteInformation currentSite))
            {
                _siteInfo.TryUpdate(site.FdbId, site, currentSite);
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
                SiteInformation data = JsonConvert.DeserializeObject<SiteInformation>(fileContent);

                // Insert the data into the MongoDB collection
                if (data != null)
                {
                    _siteInfo.TryAdd(data.FdbId, data);
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
}

