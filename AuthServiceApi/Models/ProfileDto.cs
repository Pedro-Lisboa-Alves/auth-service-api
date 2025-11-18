using System.ComponentModel.DataAnnotations;

namespace AuthServiceApi.Models
{
    public class ProfileDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        public required string Name { get; set; }
        // Adicione outras claims simples conforme necessário
    }
}