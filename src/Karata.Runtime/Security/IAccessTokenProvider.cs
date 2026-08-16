using System.IdentityModel.Tokens.Jwt;
using Karata.Kit.Cards.Models;

namespace Karata.Runtime.Security;

public interface IAccessTokenProvider
{
    public Task<string> GetAsync(CancellationToken ct = default);
    
    public virtual async Task<UserData?> CurrentUser()
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
}