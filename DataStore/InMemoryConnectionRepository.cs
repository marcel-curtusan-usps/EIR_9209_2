using EIR_9209_2.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Collections.Concurrent;

public class InMemoryConnectionRepository : IInMemoryConnectionRepository
{
    private readonly ConcurrentDictionary<string, Connection> _connectionList = new();
    private readonly ConcurrentDictionary<string, ConnectionType> _connectionTypeList = new();
    private readonly ILogger<InMemoryConnectionRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;
    private readonly string fileName = "ConnectionList.json";
    private readonly string connectionTypefileName = "ConnectionType.json";
    private readonly object _lock = new();

    public InMemoryConnectionRepository(ILogger<InMemoryConnectionRepository> logger, IHubContext<HubServices> hubServices, IConfiguration configuration, IFileService fileService)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        // Load Connection data from the first file into the first collection
        LoadDataFromFile().Wait();

        // Load ConnectionType data from the first file into the first collection
        LoadConnectionTypeDataFromFile().Wait();
    }
    public async Task<Connection>? Add(Connection connection)
    {
        bool saveToFile = false;
        try
        {
            if (_connectionList.TryAdd(connection.Id, connection))
            {
                saveToFile = true;
                return connection;
            }
            else
            {
                _logger.LogError($"Connection file was not saved...");
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
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_connectionList.Values, Formatting.Indented));
            }
        }
    }
    public async Task<Connection>? Remove(string connectionId)
    {
        bool saveToFile = false;
        try
        {
            if (_connectionList.TryRemove(connectionId, out Connection connection))
            {
                saveToFile = true;
                return await Task.FromResult(connection);
            }
            else
            {
                _logger.LogError($"Connection file was not saved...");
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
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_connectionList.Values, Formatting.Indented));
            }
        }
    }
    public async Task<Connection?> Update(Connection connection)
    {
        bool saveToFile = false;
        try
        {
            saveToFile = true;
            return _connectionList.TryGetValue(connection.Id, out Connection? currentConnection) ? currentConnection : null;
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
                await _fileService.WriteConfigurationFile(fileName, JsonConvert.SerializeObject(_connectionList.Values, Formatting.Indented));
            }
        }

    }
    public async Task<Connection?> Get(string id)
    {
        _connectionList.TryGetValue(id, out Connection connection);
        return connection;
    }

    public async Task<IEnumerable<Connection>> GetAll()
    {
        return _connectionList.Values;
    }
    public async Task<IEnumerable<Connection>> GetbyType(string type)
    {
        return _connectionList.Where(r => r.Value.Name == type).Select(y => y.Value);
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
                // Here's an example if your file was in JSON format and contained an array of T objects:
                List<Connection>? data = JsonConvert.DeserializeObject<List<Connection>>(fileContent);

                // Insert the data into the MongoDB collection
                if (data != null && data.Count != 0)
                {
                    foreach (Connection item in data.Select(r => r).ToList())
                    {
                        item.Status = EWorkerServiceState.Stopped;
                        item.LasttimeApiConnected = DateTime.MinValue;
                        _connectionList.TryAdd(item.Id, item);
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
    private async Task LoadConnectionTypeDataFromFile()
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFileFromRoot(connectionTypefileName, "Configuration");
            if (!string.IsNullOrEmpty(fileContent))
            {
                // Parse the file content to get the data. This depends on the format of your file.
                // Here's an example if your file was in JSON format and contained an array of T objects:
                List<ConnectionType>? data = JsonConvert.DeserializeObject<List<ConnectionType>>(fileContent);

                if (data != null && data.Count != 0)
                {
                    foreach (ConnectionType item in data.Select(r => r).ToList())
                    {
                        _connectionTypeList.TryAdd(item.Id, item);
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

    public async Task<ConnectionType?> AddType(ConnectionType connType)
    {
        bool saveToFile = false;
        try
        {
            if (_connectionTypeList.TryAdd(connType.Id, connType))
            {
                saveToFile = true;
                return connType;
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
                await _fileService.WriteFileInRoot(connectionTypefileName, "Configuration", JsonConvert.SerializeObject(_connectionTypeList.Values, Formatting.Indented));
            }
        }

    }
    public async Task<Messagetype?> AddSubType(string connectionId, Messagetype connection)
    {
        bool saveToFile = false;
        try
        {
            if (_connectionTypeList.TryGetValue(connectionId, out ConnectionType? currentConnectionType))
            {
                currentConnectionType.MessageTypes.Add(connection);
                saveToFile = true;
                return connection;
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
                await _fileService.WriteFileInRoot(connectionTypefileName, "Configuration", JsonConvert.SerializeObject(_connectionTypeList.Values, Formatting.Indented));
            }
        }
    }

    public async Task<ConnectionType?> RemoveType(string connTypeId)
    {
        bool saveToFile = false;
        try
        {
            if (_connectionTypeList.TryRemove(connTypeId, out ConnectionType conn))
            {
                saveToFile = true;
                return conn;
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
                await _fileService.WriteFileInRoot(connectionTypefileName, "Configuration", JsonConvert.SerializeObject(_connectionTypeList.Values, Formatting.Indented));
            }
        }
    }

    public async Task<ConnectionType> GetType(string connTypeid)
    {
        _connectionTypeList.TryGetValue(connTypeid, out ConnectionType connectiontype);
        return connectiontype;
    }

    public async Task<IEnumerable<ConnectionType>> GetTypeAll()
    {
        return _connectionTypeList.Values;
    }
    public IEnumerable<ConnectionType> GetbyNameType(string name)
    {
        return _connectionTypeList.Where(r => r.Value.Name == name).Select(y => y.Value);
    }
    public async Task<ConnectionType?> UpdateType(ConnectionType connection)
    {
        bool saveToFile = false;
        try
        {
            if (_connectionTypeList.TryGetValue(connection.Id, out ConnectionType? currentConnectionType) && _connectionTypeList.TryUpdate(connection.Id, connection, currentConnectionType))
            {
                saveToFile = true;
                return connection;
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
                await _fileService.WriteFileInRoot(connectionTypefileName, "Configuration", JsonConvert.SerializeObject(_connectionTypeList.Values, Formatting.Indented));
            }
        }
    }
    public async Task<Messagetype?> UpdateSubType(string connectionId, Messagetype connection)
    {
        bool saveToFile = false;
        try
        {
            if (_connectionTypeList.TryGetValue(connectionId, out ConnectionType? currentConnectionType))
            {
                foreach (var msgtype in currentConnectionType.MessageTypes)
                {
                    if (msgtype.Id == connection.Id)
                    {
                        if (msgtype.Description != connection.Description)
                        {
                            msgtype.Description = connection.Description;
                            saveToFile = true;
                        }
                        if (msgtype.Name != connection.Name)
                        {
                            msgtype.Name = connection.Name;
                            saveToFile = true;
                        }
                        if (msgtype.BaseURL != connection.BaseURL)
                        {
                            msgtype.BaseURL = connection.BaseURL;
                            saveToFile = true;
                        }
                    }
                }
                return connection;
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
                await _fileService.WriteFileInRoot(connectionTypefileName, "Configuration", JsonConvert.SerializeObject(_connectionTypeList.Values, Formatting.Indented));
            }
        }
    }
    public async Task<Messagetype?> RemoveSubType(string connectionId, string subId)
    {
        bool saveToFile = false;
        try
        {
            if (_connectionTypeList.TryGetValue(connectionId, out ConnectionType? currentConnectionType))
            {
                foreach (var msgtype in currentConnectionType.MessageTypes)
                {
                    if (msgtype.Id == subId)
                    {
                        currentConnectionType.MessageTypes.Remove(msgtype);
                        saveToFile = true;
                        return msgtype;
                    }
                }
                return null;
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
                await _fileService.WriteFileInRoot(connectionTypefileName, "Configuration", JsonConvert.SerializeObject(_connectionTypeList.Values, Formatting.Indented));
            }
        }
    }

    public Task<bool> ResetConnectionsList()
    {
        try
        {
            _connectionList.Clear();
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Task.FromResult(true);
        }
    }

    public Task<bool> SetupConnectionsList()
    {
        try
        {
            // Load Connection data from the first file into the first collection
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