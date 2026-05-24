using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CpmServer.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public long? UserId
    {
        get
        {
            var value = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(value, out var id))
                return id;
            return null;
        }
    }

    public string? Username => Principal?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Site => Principal?.FindFirst("Site")?.Value;

    public string? App
    {
        get
        {
            // 优先从 JWT Claim 读取
            var claim = Principal?.FindFirst("App")?.Value;
            if (!string.IsNullOrWhiteSpace(claim))
                return claim;
            // 回退到请求头
            var header = _httpContextAccessor.HttpContext?.Request.Headers["X-App-Code"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(header))
                return header;
            return "cpm"; // 默认应用
        }
    }

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role)
    {
        return Principal?.IsInRole(role) ?? false;
    }
}
