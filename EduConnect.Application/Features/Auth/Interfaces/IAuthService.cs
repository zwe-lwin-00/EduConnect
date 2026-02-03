using EduConnect.Application.Common.Interfaces;
using EduConnect.Application.DTOs.Auth;

namespace EduConnect.Application.Features.Auth.Interfaces;

public interface IAuthService : IService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string userId);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}
