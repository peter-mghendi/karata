using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Karata.Web.Infrastructure.Security;

file sealed class Route
{
    public const string Login = "authentication/login";
    public const string Logout = "authentication/logout";
    public const string Root = "/";
}

public sealed class AuthenticationHelper(NavigationManager navigator)
{
    private string CurrentPath => navigator.ToAbsoluteUri(navigator.Uri).GetLeftPart(UriPartial.Path);

    public void Login(string? returnTo = null)
    {
        var options = new InteractiveRequestOptions
        {
            Interaction = InteractionType.SignIn,
            ReturnUrl = returnTo ?? CurrentPath
        };

        navigator.NavigateToLogin(Route.Login, options);
    }

    public void Logout(string? returnTo = null) => navigator.NavigateToLogout(Route.Logout, returnTo ?? Route.Root);

    public void Register(string? returnTo = null)
    {
        var options = new InteractiveRequestOptions
        {
            Interaction = InteractionType.SignIn,
            ReturnUrl = returnTo ?? CurrentPath
        };

        options.TryAddAdditionalParameter("prompt", "create");
        navigator.NavigateToLogin(Route.Login, options);
    }

    public string Profile(string authority, string client, string? returnTo = null)
    {
        var parameters = new Dictionary<string, string>
        {
            { "referrer", client },
            { "referrer_uri", returnTo ?? CurrentPath },
        };

        var builder = new UriBuilder(authority.TrimEnd('/'));
        builder.Path += "/account";
        builder.Query = string.Join('&', parameters.Select(pair => $"{pair.Key}={pair.Value}"));

        return builder.ToString();
    }
}