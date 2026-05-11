using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>
/// 审批条件分支 —— 支持动态路由（Phase 2）
/// </summary>
[Table("SysApprovalCondition")]
public class SysApprovalCondition
{
    [Key]
    public long Id { get; set; }

    public long StepId { get; set; }

    /// <summary>
    /// 条件名称（如"高额审批"）
    /// </summary>
    public string? ConditionName { get; set; }

    /// <summary>
    /// 规则表达式，如 "Amount > 500000"
    /// </summary>
    public string? Expression { get; set; }

    /// <summary>
    /// 满足条件时跳转到的目标步骤Id
    /// </summary>
    public long? TargetStepId { get; set; }

    /// <summary>
    /// 条件优先级，数值小的优先匹配
    /// </summary>
    public int Priority { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    [ForeignKey("StepId")]
    public SysApprovalStep? Step { get; set; }
}
