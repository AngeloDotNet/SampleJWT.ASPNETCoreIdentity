using Identity.Models;

namespace Identity.BusinessLayer.Services;

public interface IIdentityService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<AuthResponse> ImpersonateAsync(Guid userId);
    Task<ChangePasswordResponse> ChangePasswordAsync(ChangePasswordRequest request);
}