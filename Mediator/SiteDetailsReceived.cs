using Mediator;

namespace EIR_9209_2.Mediator
{
    public class SiteDetailsReceived : INotification
    {
        public required string SiteName { get; init; }
    }
}