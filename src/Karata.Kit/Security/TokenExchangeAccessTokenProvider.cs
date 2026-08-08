using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IncomingAccessTokenProvider = System.Func<System.Threading.Tasks.Task<string?>>;

namespace Karata.Kit.Security;

public sealed class TokenExchangeAccessTokenProvider(
    HttpClient http,
    [FromKeyedServices(nameof(IncomingAccessTokenProvider))] IncomingAccessTokenProvider token,
    IConfiguration configuration,
    IMemoryCache cache
) : IDisposable
{
    private readonly string _audience = configuration["KARATA_ID_AUDIENCE"]!;
    private readonly string _authority = configuration["KARATA_ID_AUTHORITY"]!;
    private readonly string _client = configuration["KARATA_ID_CLIENT_ID"]!;
    private readonly string _secret = configuration["KARATA_ID_CLIENT_SECRET"]!;

    private readonly SemaphoreSlim _gate = new(1, 1);
    // private string? _token;
    // private string? _incoming;
    // private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public async Task<string> GetAsync(CancellationToken ct = default)
    {
        if (await token() is not {} incoming) throw new InvalidOperationException("Incoming user access token unavailable.");
        
        var key = Convert.ToHexString(SHA3_512.HashData(Encoding.UTF8.GetBytes(incoming)));
        if (cache.TryGetValue(key, out string? cached) && cached is [_, ..]) return cached;
        // if (_token is [_, ..] && _incoming == incoming && _expiresAt > DateTimeOffset.UtcNow) return _token;

        await _gate.WaitAsync(ct);
        try
        {
            // if (_token is [_, ..] && _incoming == incoming && _expiresAt > DateTimeOffset.UtcNow) return _token;
            if (cache.TryGetValue(key, out string? sanity) && sanity is [_, ..]) return sanity;
            
            var endpoint = $"{_authority}/protocol/openid-connect/token";
            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:token-exchange",
                ["client_id"] = _client,
                ["client_secret"] = _secret,

                ["subject_token"] = incoming,
                ["subject_token_type"] = "urn:ietf:params:oauth:token-type:access_token",

                ["requested_token_type"] = "urn:ietf:params:oauth:token-type:access_token",
                ["audience"] = _audience,
                ["scope"] = _audience
            };

            using var form = new FormUrlEncodedContent(parameters);
            using var response = await http.PostAsync(endpoint, form, ct);

            response.EnsureSuccessStatusCode();

            await using var body = await response.Content.ReadAsStreamAsync(ct);
            using var json = await JsonDocument.ParseAsync(body, cancellationToken: ct);

            var lifetime = json.RootElement.GetProperty("expires_in").GetInt32();
            var expiration = DateTimeOffset.UtcNow.AddSeconds(Math.Max(10, lifetime - 30));
            
            return cache.Set(key, json.RootElement.GetProperty("access_token").GetString()!, expiration);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose() => _gate.Dispose();
}