using Microsoft.AspNetCore.Identity;

namespace manage_library_app.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
