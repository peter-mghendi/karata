using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Karata.Runtime.Security;

public sealed class ClientCredentialsAccessTokenProvider(
    HttpClient http,
    IConfiguration configuration,
    IMemoryCache cache
) : IAccessTokenProvider, IDisposable
{
    private readonly string _authority = configuration["KARATA_ID_AUTHORITY"]!;
    private readonly string _client = configuration["KARATA_ID_CLIENT_ID"]!;
    private readonly string _secret = configuration["KARATA_ID_CLIENT_SECRET"]!;
    private readonly string _scope = configuration["KARATA_ID_SCOPE"]!;

    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<string> GetAsync(CancellationToken ct = default)
    {
        var key = Convert.ToHexString(SHA3_512.HashData(Encoding.UTF8.GetBytes($"{_client}:{_secret}")));
        if (cache.TryGetValue(key, out string? cached) && cached is [_, ..]) return cached;

        await _gate.WaitAsync(ct);
        try
        {
            if (cache.TryGetValue(key, out string? sanity) && sanity is [_, ..]) return sanity;

            var endpoint = $"{_authority}/protocol/openid-connect/token";
            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _client,
                ["client_secret"] = _secret
            };
            if (_scope is [_, ..]) parameters.Add("scope", _scope);

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