using Microsoft.EntityFrameworkCore;
using Trabajo_Final.data;
using Trabajo_Final.DTOs.Auth;
using Trabajo_Final.Helpers;
using Trabajo_Final.models;
using Trabajo_Final.Services.Interfaces;

namespace Trabajo_Final.Services
{
    public class AuthService : IAuthService
    {
        private readonly CryptoDbContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly PasswordHelper _passwordHelper;

        public AuthService(CryptoDbContext context, JwtHelper jwtHelper, PasswordHelper passwordHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
            _passwordHelper = passwordHelper;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null || !_passwordHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                return null;
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtHelper.GenerateToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            // Save session (optional)
            var session = new UserSession
            {
                UserId = user.Id,
                SessionToken = token,
                ExpiresAt = expiresAt
            };
            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = token,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
            {
                return null;
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordHelper.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtHelper.GenerateToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = token,
                ExpiresAt = expiresAt
            };
        }

        public async Task<bool> LogoutAsync(string token)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.SessionToken == token);
            if (session != null)
            {
                _context.UserSessions.Remove(session);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ValidateUserAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId && u.IsActive);
        }
    }
}
