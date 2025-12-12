using System.Security.Claims;
using Duende.IdentityModel.OidcClient;

namespace Karata.App.Services;

public sealed class AuthService(OidcClient oidc)
{
    private AuthSession? _session;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public bool IsAuthenticated => _session is not null;

    public string? AccessToken => _session?.AccessToken;

    public ClaimsPrincipal? User => _session?.User;
    
    public event EventHandler? StateChanged;

    private void OnStateChanged() => StateChanged?.Invoke(this, EventArgs.Empty);

    public async Task<bool> LoginAsync()
    {
        var result = await oidc.LoginAsync(new LoginRequest());
        if (result.IsError) return false;
        _session = new AuthSession(
            AccessToken: result.AccessToken,
            IdentityToken: result.IdentityToken,
            RefreshToken: result.RefreshToken,
            AccessTokenExpiresAt: result.AccessTokenExpiration,
            User: result.User
        );
        
        OnStateChanged();
        return true;
    }

    public async Task LogoutAsync()
    {
        if (_session?.IdentityToken is not { } id) return;
        try
        {
            await oidc.LogoutAsync(new LogoutRequest { IdTokenHint = id });
        }
        catch
        {
            /* swallow network edge */
        }

        _session = null;
        OnStateChanged();
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellation = default)
    {
        if (_session is null) return null;

        // refresh if expiring within 60s and we have a refresh token
        var needsRefresh = _session.AccessTokenExpiresAt <= DateTimeOffset.UtcNow.AddSeconds(60);
        if (!needsRefresh || string.IsNullOrWhiteSpace(_session.RefreshToken))
            return _session.AccessToken;

        await _gate.WaitAsync(cancellation);
        try
        {
            // double-check after awaiting
            if (_session is null) return null;
            if (_session.AccessTokenExpiresAt > DateTimeOffset.UtcNow.AddSeconds(60))
                return _session.AccessToken;

            var refresh = await oidc.RefreshTokenAsync(_session.RefreshToken, cancellationToken: cancellation);
            if (refresh.IsError)
            {
                _session = null;
                return null;
            }

            _session = _session with
            {
                AccessToken = refresh.AccessToken,
                IdentityToken = string.IsNullOrWhiteSpace(refresh.IdentityToken)
                    ? _session.IdentityToken
                    : refresh.IdentityToken,
                RefreshToken = string.IsNullOrWhiteSpace(refresh.RefreshToken)
                    ? _session.RefreshToken
                    : refresh.RefreshToken,
                AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(refresh.ExpiresIn)
            };

            OnStateChanged();
            return _session.AccessToken;
        }
        finally
        {
            _gate.Release();
        }
    }
}