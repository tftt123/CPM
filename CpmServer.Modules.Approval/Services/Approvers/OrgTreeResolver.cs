using CpmServer.Data;
using CpmServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Modules.Approval.Services.Approvers;

/// <summary>
/// 组织架构树解析器 —— 动态推导审批人
/// 支持：SUBMITTER_DEPT_MANAGER / SUBMITTER_DEPT_MANAGER+1
/// </summary>
public class OrgTreeResolver : IApproverResolver
{
    public string RuleType => "ORG_TREE";

    public async Task<ApproverResult> ResolveAsync(SysApprovalRule rule, ApproverContext context, CpmDbContext db)
    {
        var expression = (rule.RuleValue ?? string.Empty).Trim();

        // 获取提交人信息
        var submitter = await db.Users
            .Include(u => u.Dept)
            .FirstOrDefaultAsync(u => u.Id == context.SubmitterId);

        if (submitter == null)
            return new ApproverResult();

        // SUBMITTER_DEPT_MANAGER: 提交人的部门主管
        if (expression == "SUBMITTER_DEPT_MANAGER")
        {
            if (submitter.DeptId.HasValue)
            {
                var dept = await db.Depts
                    .FirstOrDefaultAsync(d => d.Id == submitter.DeptId.Value);

                if (dept?.ManagerId.HasValue == true)
                {
                    return new ApproverResult { UserId = dept.ManagerId.Value };
                }
            }
        }

        // SUBMITTER_DEPT_MANAGER+1: 主管的上级（再往上推一级）
        if (expression == "SUBMITTER_DEPT_MANAGER+1")
        {
            if (submitter.DeptId.HasValue)
            {
                var dept = await db.Depts
                    .Include(d => d.Manager)
                    .FirstOrDefaultAsync(d => d.Id == submitter.DeptId.Value);

                if (dept?.ManagerId.HasValue == true)
                {
                    // 找主管的部门
                    var managerDept = await db.Depts
                        .FirstOrDefaultAsync(d => d.Id == dept.Manager!.DeptId);

                    if (managerDept?.ManagerId.HasValue == true)
                    {
                        return new ApproverResult { UserId = managerDept.ManagerId.Value };
                    }
                }
            }
        }

        // 兜底：找不到时返回 RoleCode 给 fallback
        if (!string.IsNullOrEmpty(rule.Fallback))
        {
            var fallbackParts = rule.Fallback.Split(':', 2);
            if (fallbackParts.Length == 2 && fallbackParts[0] == "ROLE")
            {
                return new ApproverResult { RoleCode = fallbackParts[1] };
            }
        }

        return new ApproverResult();
    }
}
