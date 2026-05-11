using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysApprovalStep")]
public class SysApprovalStep
{
    [Key]
    public long Id { get; set; }

    public long TemplateId { get; set; }

    [Required]
    public string StepName { get; set; } = string.Empty;

    public int StepOrder { get; set; } = 1;

    /// <summary>
    /// 步骤类型：REVIEW / APPROVAL
    /// </summary>
    public string StepType { get; set; } = "REVIEW";

    /// <summary>
    /// 步骤模式：SEQUENTIAL / PARALLEL / PARALLEL_ANY / CONDITIONAL / CC
    /// </summary>
    public string StepMode { get; set; } = "SEQUENTIAL";

    /// <summary>
    /// 兼容旧字段，保持用于非规则引擎的简单审批
    /// </summary>
    public string? ApproverRole { get; set; }
    public long? ApproverUserId { get; set; }
    public bool CanReject { get; set; } = true;
    public bool CanTransfer { get; set; }
    public string? NotifyEmailTemplate { get; set; }
    public long? NextStepId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Site { get; set; }

    /// <summary>
    /// 驳回行为：REJECT_AND_CLOSE / REJECT_TO_PREV / REJECT_TO_STEP / REJECT_TO_START
    /// </summary>
    public string RejectBehavior { get; set; } = "REJECT_AND_CLOSE";

    /// <summary>
    /// 驳回目标步骤Id（当 RejectBehavior == REJECT_TO_STEP 时使用）
    /// </summary>
    public long? RejectTargetStepId { get; set; }

    /// <summary>
    /// 超时设置（小时）
    /// </summary>
    public int? TimeoutHours { get; set; }

    [ForeignKey("TemplateId")]
    public SysApprovalTemplate? Template { get; set; }

    [ForeignKey("ApproverUserId")]
    public SysUser? ApproverUser { get; set; }

    /// <summary>
    /// 审批人解析规则列表（Phase 1 新增）
    /// </summary>
    public List<SysApprovalRule> Rules { get; set; } = new();

    /// <summary>
    /// 条件分支列表（Phase 2 新增）
    /// </summary>
    public List<SysApprovalCondition> Conditions { get; set; } = new();
}
