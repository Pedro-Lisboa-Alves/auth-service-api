using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Abstractions;
using System.Threading.Tasks;

namespace AuthServiceApi.Controllers
{
    [ApiController]
    [Route("v1/connect")]
    public class OidcController : Controller
    {
        [HttpGet("authorize")]
        [AllowAnonymous]
        public IActionResult Authorize()
        {
            // O OpenIddict intercepta automaticamente esta rota e executa o fluxo Authorization Code.
            return Ok("Endpoint /connect/authorize gerido pelo OpenIddict");
        }

        [HttpPost("token")]
        [AllowAnonymous]
        public IActionResult Token()
        {
            // O OpenIddict intercepta automaticamente esta rota e executa o fluxo de troca de tokens.
            return Ok("Endpoint /connect/token gerido pelo OpenIddict");
        }

        [HttpPost("revocation")]
        [Authorize]
        public IActionResult Revocation()
        {
            // O OpenIddict intercepta automaticamente esta rota para revogação de tokens.
            return Ok("Endpoint /connect/revocation gerido pelo OpenIddict");
        }

        [Authorize]
        [HttpGet("userinfo")]
        public IActionResult UserInfo()
        {
            var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
            return Ok(new {
                sub = claims.TryGetValue(System.Security.Claims.ClaimTypes.NameIdentifier, out var sub) ? sub : null,
                email = claims.TryGetValue(System.Security.Claims.ClaimTypes.Email, out var email) ? email : null,
                name = claims.TryGetValue(System.Security.Claims.ClaimTypes.Name, out var name) ? name : null
            });
        }
    }
}
