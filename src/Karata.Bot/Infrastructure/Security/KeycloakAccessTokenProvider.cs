using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Karata.Shared.Models;

namespace Karata.Bot.Infrastructure.Security;

public sealed class KeycloakAccessTokenProvider(IHttpClientFactory factory, IConfiguration configuration) : IDisposable
{
    private readonly HttpClient _http = factory.CreateClient();
    private readonly string _clientId = configuration["Keycloak:ClientId"]!;
    private readonly string _clientSecret = configuration["Keycloak:ClientSecret"]!;
    private readonly string _authority = configuration["Keycloak:Authority"]!;
    private readonly string? _scope = configuration["Keycloak:Scope"];

    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _token;
    private UserData? _user;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    private string? CachedToken => _token is [_, ..] && DateTimeOffset.UtcNow < _expiresAt ? _token : null;

    public UserData? CurrentUser => CachedToken is null ? null : _user;

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (CachedToken is [_, ..]) return CachedToken;

        await _gate.WaitAsync(ct);
        try
        {
            if (CachedToken is [_, ..]) return CachedToken;

            var tokenEndpoint = $"{_authority}/protocol/openid-connect/token";
            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret
            };
            if (!string.IsNullOrWhiteSpace(_scope)) parameters.Add("scope", _scope!);
            using var form = new FormUrlEncodedContent(parameters);

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
            request.Content = form;

            using var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(ct);
            var json = JsonDocument.Parse(body);

            var expiresIn = json.RootElement.GetProperty("expires_in").GetInt32()!;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(10, expiresIn - 30));
            _token = json.RootElement.GetProperty("access_token").GetString()!;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(_token);

            _user = new UserData
            {
                Id = jwt.Claims.FirstOrDefault(c => c.Type == "sub")!.Value,
                Username = jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")!.Value
            };

            return _token!;
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _http.Dispose();
        _gate.Dispose();
    }
}