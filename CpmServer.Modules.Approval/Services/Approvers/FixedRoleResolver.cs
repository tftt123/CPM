using CpmServer.Data;
using CpmServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Modules.Approval.Services.Approvers;

/// <summary>
/// 固定角色解析器 —— 兼容现有逻辑
/// </summary>
public class FixedRoleResolver : IApproverResolver
{
    public string RuleType => "FIXED_ROLE";

    public Task<ApproverResult> ResolveAsync(SysApprovalRule rule, ApproverContext context, CpmDbContext db)
    {
        if (!string.IsNullOrEmpty(rule.RuleValue))
        {
            return Task.FromResult(new ApproverResult { RoleCode = rule.RuleValue.Trim() });
        }

        return Task.FromResult(new ApproverResult());
    }
}
