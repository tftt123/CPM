using Hangfire.Dashboard;
using System.Security.Claims;
using CpmServer.Authorization;

namespace CpmServer.Common;

/// <summary>
/// Hangfire 仪表盘权限认证过滤器
/// 要求用户已登录且拥有 system.manage 权限或 ADMIN 角色
/// </summary>
public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var user = httpContext.User;

        if (user.Identity?.IsAuthenticated != true)
            return false;

        var hasPermission = user.Claims
            .Any(c => c.Type == "Permission" && c.Value == Permissions.CanManageSystem);

        var isAdmin = user.Claims
            .Any(c => c.Type == ClaimTypes.Role && c.Value.ToUpperInvariant() == "ADMIN");

        return hasPermission || isAdmin;
    }
}
