using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Mvc;

namespace Karata.Server.Controllers;

public class OidcConfigurationController(IClientRequestParametersProvider provider) : Controller
{
    [HttpGet("_configuration/{clientId}")]
    public IActionResult GetClientRequestParameters([FromRoute]string clientId) => 
        Ok(provider.GetClientParameters(HttpContext, clientId));
}
