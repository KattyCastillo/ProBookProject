using G6.ProBook.WebApi.DTOs;
using G6.ProBook.WebApi.Models;

namespace G6.ProBook.WebApi.Services
{
    public interface IAuthService
    {
        Task<User> Register(RegisterDto registerDto);

        Task<(User user, string token)> Login(LoginDto loginDto);

        Task<bool> ValidateToken(string token);

        Task<User?> GetUserById(string userId);

        string GenerateJwtToken(User user);
    }
}
