using Hangfire.Dashboard;

namespace CpmServer.Common;

/// <summary>
/// Hangfire 仪表盘基础认证过滤器
/// </summary>
public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // 开发环境允许访问，生产环境建议增加更严格的认证
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true;
    }
}
