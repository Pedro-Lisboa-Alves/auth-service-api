using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AuthServiceApi.Models;
using System.Threading.Tasks;

namespace AuthServiceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

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
    }
}