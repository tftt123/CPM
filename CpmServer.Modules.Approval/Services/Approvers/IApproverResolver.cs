using CpmServer.Data;
using CpmServer.Models;

namespace CpmServer.Modules.Approval.Services.Approvers;

/// <summary>
/// 审批人解析上下文
/// </summary>
public class ApproverContext
{
    /// <summary>发起流程的用户Id</summary>
    public long SubmitterId { get; set; }

    /// <summary>发起人Site</summary>
    public string? SubmitterSite { get; set; }

    /// <summary>业务单据关联客户Id</summary>
    public long? CustomerId { get; set; }

    /// <summary>业务金额</summary>
    public decimal? Amount { get; set; }

    /// <summary>当前审批实例Id</summary>
    public long? InstanceId { get; set; }
}

/// <summary>
/// 审批人解析结果
/// </summary>
public class ApproverResult
{
    /// <summary>精确到用户的Id（null 表示角色审批）</summary>
    public long? UserId { get; set; }

    /// <summary>角色代码</summary>
    public string? RoleCode { get; set; }

    /// <summary>解析状态</summary>
    public bool Success => UserId.HasValue || !string.IsNullOrEmpty(RoleCode);
}

/// <summary>
/// 审批人解析器接口 —— Phase 1 新增
/// 支持 FIXED_ROLE / FIXED_USER / ORG_TREE / EXPRESSION
/// </summary>
public interface IApproverResolver
{
    /// <summary>规则类型</summary>
    string RuleType { get; }

    /// <summary>解析审批人</summary>
    Task<ApproverResult> ResolveAsync(SysApprovalRule rule, ApproverContext context, CpmDbContext db);
}
