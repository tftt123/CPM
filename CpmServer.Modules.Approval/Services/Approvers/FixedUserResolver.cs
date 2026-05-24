using CpmServer.Constants;
using CpmServer.Data;
using CpmServer.Models;

namespace CpmServer.Modules.Approval.Services.Approvers;

/// <summary>
/// 固定用户解析器 —— 兼容现有逻辑
/// </summary>
public class FixedUserResolver : IApproverResolver
{
    public string RuleType => ApprovalConstants.RuleType.FixedUser;

    public Task<ApproverResult> ResolveAsync(SysApprovalRule rule, ApproverContext context, CpmDbContext db)
    {
        if (long.TryParse(rule.RuleValue, out var userId))
        {
            return Task.FromResult(new ApproverResult { UserId = userId });
        }

        return Task.FromResult(new ApproverResult());
    }
}
