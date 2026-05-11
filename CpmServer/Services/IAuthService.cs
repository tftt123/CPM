using CpmServer.DTOs.Auth;

namespace CpmServer.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<List<string>> GetUserSitesAsync(long userId);
    Task<LoginResponse> SwitchSiteAsync(long userId, string site);
}