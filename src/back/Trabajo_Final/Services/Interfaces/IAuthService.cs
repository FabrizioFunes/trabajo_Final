using Trabajo_Final.DTOs.Auth;

namespace Trabajo_Final.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<bool> LogoutAsync(string token);
        Task<bool> ValidateUserAsync(int userId);
    }
}
