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
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // Check for permission claim
        var hasPermission = context.User.Claims
            .Any(c => c.Type == "Permission" && c.Value == requirement.Permission);

        // Fallback: ADMIN role always has all permissions (backward compatibility)
        var isAdmin = context.User.Claims
            .Any(c => c.Type == ClaimTypes.Role && c.Value.ToUpperInvariant() == "ADMIN");

        if (hasPermission || isAdmin)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
