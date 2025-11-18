using System.ComponentModel.DataAnnotations;

namespace AuthServiceApi.Models
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}