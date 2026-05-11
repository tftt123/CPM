using CpmServer.Data;
using CpmServer.Models;

namespace CpmServer.Modules.Approval.Services.Approvers;

/// <summary>
/// 提交人解析器 —— 退回给发起人时自动解析为实际提交人
/// 规则值：REQUESTOR
/// </summary>
public class SubmitterResolver : IApproverResolver
{
    public string RuleType => "SUBMITTER";

    public Task<ApproverResult> ResolveAsync(SysApprovalRule rule, ApproverContext context, CpmDbContext db)
    {
        if (context.SubmitterId > 0)
        {
            return Task.FromResult(new ApproverResult { UserId = context.SubmitterId });
        }

        return Task.FromResult(new ApproverResult());
    }
}
