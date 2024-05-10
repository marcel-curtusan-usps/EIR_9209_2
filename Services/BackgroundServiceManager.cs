public class BackgroundServiceManager
{
    private readonly Dictionary<string, ConnectionBackgroundService> _services = new();


    public ConnectionBackgroundService GetService(string id)
    {
        return _services.ContainsKey(id) ? _services[id] : null;
    }
    public IEnumerable<string> GetServiceIds()
    {
        return _services.Keys;
    }
    public void AddService(string id, ConnectionBackgroundService service)
    {
        _services[id] = service;
    }

    public void RemoveService(string id)
    {
        if (_services.ContainsKey(id))
        {
            _services.Remove(id);
        }
    }

    public async Task StartServiceAsync(string id, CancellationToken cancellationToken)
    {
        var service = GetService(id);
        if (service != null)
        {
            await service.StartAsync(cancellationToken);
        }
    }

    public async Task StopServiceAsync(string id, CancellationToken cancellationToken)
    {
        var service = GetService(id);
        if (service != null)
        {
            await service.StopAsync(cancellationToken);
        }
    }
}
