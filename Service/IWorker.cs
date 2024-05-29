namespace EIR_9209_2.Service
{
    public interface IWorker
    {
        bool AddEndpoint(Connection connection);
        bool RemoveEndpoint(Connection connection);
        bool UpdateEndpoint(Connection connection);

    }
}