using System.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;

namespace Karata.Client.Infrastructure.Security;

public sealed class AuthenticationHelper(NavigationManager navigator, IAccessTokenProvider provider, IJSRuntime js)
{
    private string ReturnUrl => navigator.ToAbsoluteUri(navigator.Uri).GetLeftPart(UriPartial.Path);

    public void Login(string? returnUrl = null)
        => navigator.NavigateTo($"authentication/login?returnUrl={Uri.EscapeDataString(returnUrl ?? ReturnUrl)}");

    public void Register(string clientId, string authority, string redirectUri)
    {
        var url =
            $"{authority}/protocol/openid-connect/registrations" +
            $"?client_id={Uri.EscapeDataString(clientId)}" +
            $"&response_type=code" +
            $"&scope=openid%20profile%20email" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";
        navigator.NavigateTo(url, forceLoad: true);
    }

    public async Task OpenProfileAsync(string realmAuthority, string clientId, string? referrerUri = null)
    {
        var refUri = referrerUri ?? ReturnUrl;
        var url = $"{realmAuthority}/account?referrer={HttpUtility.UrlEncode(clientId)}&referrer_uri={HttpUtility.UrlEncode(refUri)}";
        await js.InvokeVoidAsync("open", url, "_blank");
    }

    public void Logout(string? returnUrl = null)
        => navigator.NavigateTo($"authentication/logout?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
}
