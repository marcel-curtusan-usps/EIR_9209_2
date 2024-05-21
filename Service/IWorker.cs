public interface IWorker
{
    void AddEndpoint(Connection endpointConfig);
    void RemoveEndpoint(string id);
    void UpdateEndpointInterval(Connection updateConfig);
    void UpdateEndpointActive(Connection updateConfig);
}