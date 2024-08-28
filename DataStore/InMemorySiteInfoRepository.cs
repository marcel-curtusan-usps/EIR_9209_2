using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace EIR_9209_2.DataStore
{
    public class InMemorySiteInfoRepository : IInMemorySiteInfoRepository
    {
        private readonly static ConcurrentDictionary<string, SiteInformation> _siteInfo = new();
        private readonly ILogger<InMemorySiteInfoRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileService FileService;
        private readonly string filePath = "";
        private readonly string fileName = "";
        public InMemorySiteInfoRepository(ILogger<InMemorySiteInfoRepository> logger, IConfiguration configuration, IFileService fileService)
        {
            FileService = fileService;
            _logger = logger;
            _configuration = configuration;
            fileName = $"{_configuration[key: "InMemoryCollection:CollectionSiteInformation"]}.json";
            filePath = Path.Combine(configuration[key: "ApplicationConfiguration:BaseDrive"],
                configuration[key: "ApplicationConfiguration:BaseDirectory"],
                configuration[key: "ApplicationConfiguration:NassCode"],
                configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
                $"{fileName}");
            // Load data from the first file into the first collection
            _ = LoadDataFromFile(filePath);

        }
        public void Add(SiteInformation site)
        {
            if (_siteInfo.TryAdd(site.FdbId, site))
            {
                FileService.WriteFile(fileName, JsonConvert.SerializeObject(_siteInfo.Values, Formatting.Indented));
            }
        }

        public void Remove(string id)
        {
            if (_siteInfo.TryRemove(id, out _))
            {
                FileService.WriteFile(fileName, JsonConvert.SerializeObject(_siteInfo.Values, Formatting.Indented));
            }
        }

        public SiteInformation Get(string id)
        {
            _siteInfo.TryGetValue(id, out SiteInformation tag);
            return tag;
        }
        public SiteInformation GetSiteInfo()
        {
            return _siteInfo.Values.FirstOrDefault();
        }

        public void Update(SiteInformation site)
        {
            if (_siteInfo.TryGetValue(site.FdbId, out SiteInformation currentSite) && _siteInfo.TryUpdate(site.FdbId, site, currentSite))
            {

                FileService.WriteFile(fileName, JsonConvert.SerializeObject(_siteInfo.Values, Formatting.Indented));
            }
        }
        private DateTime GetCurrentTimeInTimeZone(string timeZoneId)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime currentTime = DateTime.Now;
            DateTime currentTimeInTimeZone = TimeZoneInfo.ConvertTime(currentTime, timeZone);
            return currentTimeInTimeZone;
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

        public DateTime GetCurrentTimeInTimeZone(DateTime currentTime)
        {
            string timeZoneId = "";
            try
            {
                if (!CustomTimeZoneMappings.TryGetValue(_siteInfo.Values.FirstOrDefault().TimeZoneAbbr, out timeZoneId))
                {
                    throw new ArgumentException($"Invalid timezone identifier: {_siteInfo.Values.FirstOrDefault().TimeZoneAbbr}");
                }
                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime currentTimeInTimeZone = TimeZoneInfo.ConvertTime(currentTime, timeZone);
                return currentTimeInTimeZone;
            }
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine($"The timezone identifier '{timeZoneId}' was not found.");
                // Handle the error or return a default value
                return DateTime.Now; // Default to local time
            }
            catch (InvalidTimeZoneException)
            {
                Console.WriteLine($"The timezone identifier '{timeZoneId}' is invalid.");
                // Handle the error or return a default value
                return DateTime.Now; // Default to local time
            }
        }
        private static readonly Dictionary<string, string> CustomTimeZoneMappings = new Dictionary<string, string>
            {
            { "EST", "America/New_York" },
            { "CST", "America/Chicago" },
            { "MST", "America/Denver" },
            { "PST", "America/Los_Angeles" },
            { "AKST", "America/Anchorage" },
            { "HST", "Pacific/Honolulu" },
            { "EST1", "America/New_York" },
            { "CST1", "America/Chicago" },
            { "MST1", "America/Denver" },
            { "PST1", "America/Los_Angeles" },
            { "AKST1", "America/Anchorage" },
            { "HST1", "Pacific/Honolulu" }
                // Add other mappings as needed
            };
    }

}

