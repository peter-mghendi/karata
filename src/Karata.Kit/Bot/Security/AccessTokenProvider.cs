using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Karata.Kit.Cards.Models;
using Microsoft.Extensions.Configuration;

namespace Karata.Kit.Bot.Security;

public sealed class AccessTokenProvider(HttpClient http, IConfiguration configuration) : IDisposable
{
    private readonly string _authority = configuration["KARATA_ID_AUTHORITY"]!;
    private readonly string _client = configuration["KARATA_ID_CLIENT_ID"]!;
    private readonly string _secret = configuration["KARATA_ID_CLIENT_SECRET"]!;
    private readonly string? _scope = configuration["KARATA_ID_SCOPE"];

    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _token;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public async Task<UserData?> CurrentUser()
    {
        if (await GetAsync() is not { } token) return null;
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        return new UserData
        {
            Id = jwt.Claims.FirstOrDefault(c => c.Type == "sub")!.Value,
            Username = jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")!.Value
        };
    }

    public async Task<string> GetAsync(CancellationToken ct = default)
    {
        if (_token is [_, ..] && DateTimeOffset.UtcNow < _expiresAt) return _token;

        await _gate.WaitAsync(ct);
        try
        {
            if (_token is [_, ..] && DateTimeOffset.UtcNow < _expiresAt) return _token;

            var tokenEndpoint = $"{_authority}/protocol/openid-connect/token";
            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _client,
                ["client_secret"] = _secret
            };
            if (!string.IsNullOrWhiteSpace(_scope)) parameters.Add("scope", _scope!);
            using var form = new FormUrlEncodedContent(parameters);

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
            request.Content = form;

            using var response = await http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(ct);
            var json = JsonDocument.Parse(body);

            var expiresIn = json.RootElement.GetProperty("expires_in").GetInt32();
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(10, expiresIn - 30));
            _token = json.RootElement.GetProperty("access_token").GetString()!;

            return _token!;
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose() => _gate.Dispose();
}