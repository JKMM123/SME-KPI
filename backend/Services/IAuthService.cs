using SmeKpiDashboard.DTOs;

namespace SmeKpiDashboard.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest dto);
    Task<AuthResponse> LoginAsync(LoginRequest dto);
    Task<AuthResponse> RefreshAsync(RefreshRequest dto);
    Task RevokeAsync(Guid userId);
}
