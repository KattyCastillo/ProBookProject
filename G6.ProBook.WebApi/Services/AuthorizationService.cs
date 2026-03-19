using G6.ProBook.WebApi.Models;

namespace G6.ProBook.WebApi.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(IAuthService authService, ILogger<AuthorizationService> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<bool> UserHasRoleAsync(string userId, string requiredRole)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(requiredRole))
                {
                    return false;
                }

                var user = await _authService.GetUserById(userId);
                return user != null && user.Role.Equals(requiredRole, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validando rol: {ex.Message}");
                return false;
            }
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return null;
                }

                return await _authService.GetUserById(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error obteniendo usuario: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsManagerAsync(string userId)
        {
            return await UserHasRoleAsync(userId, "manager");
        }

        public async Task<bool> IsGuestAsync(string userId)
        {
            return await UserHasRoleAsync(userId, "huesped");
        }
    }
}
