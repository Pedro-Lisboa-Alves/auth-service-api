using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AuthServiceApi.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace AuthServiceApi.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous] // Permite acesso sem autenticação
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

            // TODO: Gerar e devolver token JWT/OIDC
            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
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

        [AllowAnonymous] // Permite acesso sem autenticação
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
                return Ok();

            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);

            return BadRequest(ModelState);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Ok(); // Não revelar se o email existe

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // TODO: Enviar email com o token
            return Ok();
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            Console.WriteLine("[LOG] Entrou no método GetProfile");
            Console.WriteLine($"[LOG] User.Identity.IsAuthenticated={User?.Identity?.IsAuthenticated}");
            Console.WriteLine($"[LOG] User.Identity.Name={User?.Identity?.Name}");
            if (User?.Claims != null)
            {
                foreach (var c in User.Claims)
                {
                    Console.WriteLine($"[LOG] Principal Claim: {c.Type} = {c.Value}");
                }
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Console.WriteLine("[LOG] Usuário não encontrado ou não autenticado");
                return Unauthorized();
            }

            var profile = new ProfileDto
            {
                Email = user.Email ?? string.Empty,
                Name = user.UserName ?? string.Empty // ou outra claim
            };
            Console.WriteLine($"[LOG] Retornando perfil: Email={profile.Email}, Name={profile.Name}");
            // TODO: Adicionar claims simples se necessário
            return Ok(profile);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
                return Ok();

            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);

            return BadRequest(ModelState);
        }

    }
}