using EIR_9209_2.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;

public class InMemoryConnectionRepository : IInMemoryConnectionRepository
{
    private readonly ConcurrentDictionary<string, Connection> _connectionList = new();
    private readonly ConcurrentDictionary<string, ConnectionType> _connectionTypeList = new();
    private readonly ILogger<InMemoryConnectionRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;
    private readonly string filePath = "";
    private readonly string fileName = "";

    public InMemoryConnectionRepository(ILogger<InMemoryConnectionRepository> logger, IConfiguration configuration, IFileService fileService)
    {
        _fileService = fileService;
        _logger = logger;
        _configuration = configuration;
        fileName = $"{_configuration[key: "InMemoryCollection:CollectionConnections"]}.json";
        filePath = Path.Combine(_configuration[key: "ApplicationConfiguration:BaseDrive"],
            _configuration[key: "ApplicationConfiguration:BaseDirectory"],
            _configuration[key: "ApplicationConfiguration:NassCode"],
            _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"],
            $"{fileName}");
        // Load Connection data from the first file into the first collection
        _ = LoadDataFromFile(filePath);

        string conTypeFilePath = Path.Combine(Directory.GetCurrentDirectory(), _configuration[key: "ApplicationConfiguration:ConfigurationDirectory"], $"{"ConnectionType"}.json");
        // Load ConnectionType data from the first file into the first collection
        _ = LoadConnectionTypeDataFromFile(conTypeFilePath);
    }
    public Connection? Add(Connection connection)
    {
        if (_connectionList.TryAdd(connection.Id, connection))
        {
            if (_fileService.WriteFile("ConnectionList.json", JsonConvert.SerializeObject(_connectionList.Values, Formatting.Indented)))
            {
                return connection;
            }
            else
            {
                _logger.LogError($"ConnectionList.json was not update");
                return null;

            }

        }
        else
        {
            return null;
        }
    }
    public Connection? Remove(string connectionId)
    {
        if (_connectionList.TryRemove(connectionId, out Connection conn))
        {
            if (_fileService.WriteFile("ConnectionList.json", JsonConvert.SerializeObject(_connectionList.Values, Formatting.Indented)))
            {
                return conn;
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
    public Connection? Update(Connection connection)
    {


        if (_connectionList.TryGetValue(connection.Id, out Connection? currentConnection) && _connectionList.TryUpdate(connection.Id, connection, currentConnection))
        {
            if (_fileService.WriteFile("ConnectionList.json", JsonConvert.SerializeObject(_connectionList.Values, Formatting.Indented)))
            {
                return Get(connection.Id);
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
    public Connection Get(string id)
    {
        _connectionList.TryGetValue(id, out Connection connection);
        return connection;
    }

    public IEnumerable<Connection> GetAll()
    {
        return _connectionList.Values;
    }
    public IEnumerable<Connection> GetbyType(string type)
    {
        return _connectionList.Where(r => r.Value.Name == type).Select(y => y.Value);
    }
    /// <summary>
    /// Updates a connection in the in-memory connection repository.
    /// </summary>
    /// <param name="connection">The connection to update.</param>

    private async Task LoadDataFromFile(string filePath)
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(filePath);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            List<Connection> data = JsonConvert.DeserializeObject<List<Connection>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data != null && data.Count != 0)
            {
                foreach (Connection item in data.Select(r => r).ToList())
                {
                    _connectionList.TryAdd(item.Id, item);
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


    public ConnectionType? AddType(ConnectionType connType)
    {
        if (_connectionTypeList.TryAdd(connType.Id, connType))
        {
            if (_fileService.WriteFileInAppConfig("ConnectionType.json", JsonConvert.SerializeObject(_connectionTypeList.Values, Formatting.Indented)))
            {
                return connType;
            }
            else
            {
                _logger.LogError($"ConnectionType.json was not update");
                return null;

            }

        }
        else
        {
            return null;
        }
    }
    public Messagetype? AddSubType(string connectionId, Messagetype connection)
    {
        if (_connectionTypeList.TryGetValue(connectionId, out ConnectionType? currentConnectionType))
        {
            currentConnectionType.MessageTypes.Add(connection);
            if (_fileService.WriteFileInAppConfig("ConnectionType.json", JsonConvert.SerializeObject(_connectionTypeList.Values, Formatting.Indented)))
            {
                return connection;
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

    public ConnectionType? RemoveType(string connTypeId)
    {
        if (_connectionTypeList.TryRemove(connTypeId, out ConnectionType conn))
        {
            if (_fileService.WriteFileInAppConfig("ConnectionType.json", JsonConvert.SerializeObject(_connectionTypeList.Values, Formatting.Indented)))
            {
                return conn;
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

    public ConnectionType GetType(string connTypeid)
    {
        _connectionTypeList.TryGetValue(connTypeid, out ConnectionType connectiontype);
        return connectiontype;
    }

    public IEnumerable<ConnectionType> GetTypeAll()
    {
        return _connectionTypeList.Values;
    }
    public IEnumerable<ConnectionType> GetbyNameType(string name)
    {
        return _connectionTypeList.Where(r => r.Value.Name == name).Select(y => y.Value);
    }
    public ConnectionType? UpdateType(ConnectionType connection)
    {
        if (_connectionTypeList.TryGetValue(connection.Id, out ConnectionType? currentConnectionType) && _connectionTypeList.TryUpdate(connection.Id, connection, currentConnectionType))
        {
            if (_fileService.WriteFileInAppConfig("ConnectionType.json", JsonConvert.SerializeObject(_connectionTypeList.Values, Formatting.Indented)))
            {
                return GetType(connection.Id);
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
    public Messagetype? UpdateSubType(string connectionId, Messagetype connection)
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
                    }
                    if (msgtype.Name != connection.Name)
                    {
                        msgtype.Name = connection.Name;
                    }
                }
            }
            if(_fileService.WriteFileInAppConfig("ConnectionType.json", JsonConvert.SerializeObject(_connectionTypeList.Values, Formatting.Indented)))
            { 
                return connection;
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
    public Messagetype? RemoveSubType(string connectionId, string subId)
    {
        if (_connectionTypeList.TryGetValue(connectionId, out ConnectionType? currentConnectionType))
        {
            foreach (var msgtype in currentConnectionType.MessageTypes)
            {
                if (msgtype.Id == subId)
                {
                    currentConnectionType.MessageTypes.Remove(msgtype);
                    _fileService.WriteFileInAppConfig("ConnectionType.json", JsonConvert.SerializeObject(_connectionTypeList.Values, Formatting.Indented));
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

    private async Task LoadConnectionTypeDataFromFile(string conTypeFilePath)
    {
        try
        {
            // Read data from file
            var fileContent = await _fileService.ReadFile(conTypeFilePath);

            // Parse the file content to get the data. This depends on the format of your file.
            // Here's an example if your file was in JSON format and contained an array of T objects:
            List<ConnectionType> data = JsonConvert.DeserializeObject<List<ConnectionType>>(fileContent);

            // Insert the data into the MongoDB collection
            if (data.Count != 0)
            {
                foreach (ConnectionType item in data.Select(r => r).ToList())
                {
                    _connectionTypeList.TryAdd(item.Id, item);
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