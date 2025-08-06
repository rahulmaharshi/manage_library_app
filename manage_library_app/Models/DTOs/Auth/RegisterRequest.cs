using System.ComponentModel.DataAnnotations;

namespace manage_library_app.Models.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be greater than or equal to 8 characters.")]
        public string Password { get; set; }

        [Required]
        public string FullName { get; set; }
    }
}
