public interface IBackgroundServiceManager
{
    ConnectionBackgroundService GetService(string id);
    IEnumerable<string> GetServiceIds();
    void AddService(string id, ConnectionBackgroundService service);
    void RemoveService(string id);
    Task StartServiceAsync(string id, CancellationToken cancellationToken);
    Task StopServiceAsync(string id, CancellationToken cancellationToken);
}