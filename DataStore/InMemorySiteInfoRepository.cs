using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace EIR_9209_2.DataStore
{
    public class InMemorySiteInfoRepository : IInMemorySiteInfoRepository
    {
        private readonly static ConcurrentDictionary<string, SiteInformation> _siteInfo = new();
        private readonly ILogger<InMemorySiteInfoRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;
        private readonly string fileName = "SiteInformation.json";

        public InMemorySiteInfoRepository(ILogger<InMemorySiteInfoRepository> logger, IConfiguration configuration, IFileService fileService)
        {
            _fileService = fileService;
            _logger = logger;
            _configuration = configuration;
            // Load data from the first file into the first collection
            LoadDataFromFile().Wait();
        }

        public void Add(SiteInformation site)
        {
            if (_siteInfo.TryAdd(site.FdbId, site))
            {
                _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_siteInfo.Values.FirstOrDefault(), Formatting.Indented));

            }
        }

        public void Remove(string id)
        {
            if (_siteInfo.TryRemove(id, out _))
            {
                _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_siteInfo.Values.FirstOrDefault(), Formatting.Indented));
            }
        }

        public SiteInformation Get(string id)
        {
            _siteInfo.TryGetValue(id, out SiteInformation tag);
            return tag;
        }
        public Task<SiteInformation>? GetSiteInfo()
        {
            try
            {
                var info = _siteInfo.Values.FirstOrDefault();
                info ??= new SiteInformation();
                return Task.FromResult(info);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }

        }

        public void Update(SiteInformation site)
        {
            if (_siteInfo.TryGetValue(site.FdbId, out SiteInformation currentSite) && _siteInfo.TryUpdate(site.FdbId, site, currentSite))
            {

                _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_siteInfo.Values.FirstOrDefault(), Formatting.Indented));
            }
        }

        private async Task LoadDataFromFile()
        {
            try
            {
                // Read data from file
                var fileContent = await _fileService.ReadFile(fileName);
                if (!string.IsNullOrEmpty(fileContent))
                {
                    // Parse the file content to get the data. This depends on the format of your file.
                    SiteInformation? data = JsonConvert.DeserializeObject<SiteInformation>(fileContent);

                    // Insert the data into the MongoDB collection
                    if (data != null)
                    {
                        _siteInfo.TryAdd(data.FdbId, data);
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

        public async Task<DateTime> GetCurrentTimeInTimeZone(DateTime currentTime)
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

        public Task<bool> ResetSiteInfoList()
        {
            try
            {
                _siteInfo.Clear();
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Task.FromResult(true);
            }
        }

        public Task<bool> SetupSiteInfoList()
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

        private static readonly Dictionary<string, string> CustomTimeZoneMappings = new()
        {
            { "EST", "America/New_York" },
            { "CST", "America/Chicago" },
            { "MST", "America/Denver" },
            { "PST", "America/Los_Angeles" },
            { "AKST", "America/Anchorage" },
            { "HST", "Pacific/Honolulu" },
            { "EST1", "America/New_York" },
            { "EST2", "America/New_York" },
            { "CST1", "America/Chicago" },
            { "CST2", "America/Chicago" },
            { "MST1", "America/Denver" },
            { "MST2","America/Denver"},
            { "PST1", "America/Los_Angeles" },
            { "PST2", "America/Los_Angeles" },
            { "AKST1", "America/Anchorage" },
            { "AKST2", "America/Anchorage" },
            { "HST1", "Pacific/Honolulu" },
            { "HST2", "Pacific/Honolulu" }
            };
    }

}

