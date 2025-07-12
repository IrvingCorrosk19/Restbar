using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Interfaces
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<bool> IsUserActiveAsync(Guid userId);
        Task<bool> HasPermissionAsync(Guid userId, string action);
        Task<string> GenerateJwtTokenAsync(User user);
        Task<ClaimsPrincipal> GetClaimsPrincipalAsync(User user);
        Task LogoutAsync(string userId);
        Task<User> CreateDefaultAdminAsync();
        Task<bool> HasAdminUserAsync();
    }
} 