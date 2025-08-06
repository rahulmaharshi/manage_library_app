using System.ComponentModel.DataAnnotations;

namespace manage_library_app.Models.DTOs.Auth
{
    public class CreateUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải lớn hơn hoặc bằng 8 ký tự.")]
        public string Password { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [RegularExpression("^(Librarian|Member)$", ErrorMessage = "Vai trò không hợp lệ. Chỉ chấp nhận 'Librarian' hoặc 'Member'.")]
        public string Role { get; set; } // Role: Librarian hoặc Member
    }
}
