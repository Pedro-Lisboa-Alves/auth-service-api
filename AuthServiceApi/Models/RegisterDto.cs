using System.ComponentModel.DataAnnotations;

namespace AuthServiceApi.Models
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(6)]
        public required string Password { get; set; }

        // Adicione outros campos conforme necessário
    }
}