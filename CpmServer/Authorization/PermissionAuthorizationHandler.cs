using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CpmServer.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // Check for permission claim
        var hasPermission = context.User.Claims
            .Any(c => c.Type == "Permission" && c.Value == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Fallback: ADMIN role always has all permissions (backward compatibility)
        // TODO: 商业化后移除此后备，要求 ADMIN 也必须在 SysRolePermission 中有对应权限
        var isAdmin = context.User.Claims
            .Any(c => c.Type == ClaimTypes.Role && c.Value.ToUpperInvariant() == "ADMIN");

        if (isAdmin)
        {
            _logger.LogWarning(
                "ADMIN fallback triggered for permission {Permission}. User={User}. " +
                "Please assign this permission via SysRolePermission and remove ADMIN fallback.",
                requirement.Permission,
                context.User.Identity?.Name ?? "unknown");
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
