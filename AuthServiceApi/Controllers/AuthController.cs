using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using AuthServiceApi.Models;
using System;
using System.Linq;

namespace AuthServiceApi.Controllers
{
    [ApiController]
    [Route("v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("Credenciais inválidas.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Credenciais inválidas.");

            var accessToken = GenerateJwtToken(user);
            var refreshToken = Guid.NewGuid().ToString(); // Simples, para exemplo
            var expiresIn = 3600;

            // TODO: Salvar refreshToken no banco associado ao usuário

            return Ok(new {
                accessToken,
                refreshToken,
                expiresIn
            });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshDto model)
        {
            // TODO: Validar refreshToken no banco e obter usuário
            // Exemplo: sempre retorna o mesmo usuário para teste
            var user = await _userManager.FindByEmailAsync("test@example.com");
            if (user == null)
                return Unauthorized();

            var accessToken = GenerateJwtToken(user);
            var refreshToken = Guid.NewGuid().ToString();
            var expiresIn = 3600;

            return Ok(new {
                accessToken,
                refreshToken,
                expiresIn
            });
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var key = System.Text.Encoding.ASCII.GetBytes("super_secret_jwt_key_change_this");
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim("sub", user.Id)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class RefreshDto
    {
        public string? RefreshToken { get; set; }
    }
}
