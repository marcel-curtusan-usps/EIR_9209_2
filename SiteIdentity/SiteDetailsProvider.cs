using Microsoft.Extensions.Options;
using Mediator;
using EIR_9209_2.Mediator;

namespace EIR_9209_2.SiteIdentity
{
    public class SiteDetailsProvider(IOptions<SiteIdentitySettings> siteSettings) : INotificationHandler<SiteDetailsReceived>, ISiteDetailsProvider
    {
        private string? _siteName;

        public string GetSiteName()
        {
            return _siteName ?? siteSettings.Value.NassCode;
        }

        public ValueTask Handle(SiteDetailsReceived siteDetailsReceived, CancellationToken cancellationToken)
        {
            _siteName = siteDetailsReceived.SiteName;
            return default;
        }
    }
}
