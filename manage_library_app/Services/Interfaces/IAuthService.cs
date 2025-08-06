using manage_library_app.Models.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace manage_library_app.Services.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterMemberAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<UserInfo> GetUserInfoAsync(string userId);
        Task<IdentityResult> CreateUserByAdminAsync(CreateUserRequest request);
    }
}
