public interface IOAuth2AuthenticationService
{
    Task AddAuthHeader(HttpRequestMessage request, CancellationToken ct);
}