namespace CpmServer.Modules.Approval.DTOs;

public class ApprovalTemplateDto
{
    public long? Id { get; set; }
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string ModuleType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string? Site { get; set; }
    public List<ApprovalStepConfigDto> Steps { get; set; } = new();
}

public class ApprovalStepConfigDto
{
    public long? Id { get; set; }
    public string StepName { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public string StepType { get; set; } = "REVIEW";

    /// <summary>
    /// 步骤模式：SEQUENTIAL / PARALLEL / PARALLEL_ANY / CONDITIONAL / CC
    /// </summary>
    public string StepMode { get; set; } = "SEQUENTIAL";

    public string? ApproverRole { get; set; }
    public long? ApproverUserId { get; set; }
    public bool CanReject { get; set; } = true;
    public bool CanTransfer { get; set; }
    public string? NotifyEmailTemplate { get; set; }

    /// <summary>
    /// 驳回行为：REJECT_AND_CLOSE / REJECT_TO_PREV / REJECT_TO_STEP / REJECT_TO_START
    /// </summary>
    public string RejectBehavior { get; set; } = "REJECT_AND_CLOSE";

    public long? RejectTargetStepId { get; set; }

    /// <summary>
    /// 超时设置（小时）
    /// </summary>
    public int? TimeoutHours { get; set; }

    /// <summary>
    /// 审批人解析规则列表
    /// </summary>
    public List<ApprovalRuleDto> Rules { get; set; } = new();

    /// <summary>
    /// 条件分支列表（Phase 2）
    /// </summary>
    public List<ApprovalConditionDto> Conditions { get; set; } = new();
}

public class ApprovalRuleDto
{
    public long? Id { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string? RuleValue { get; set; }
    public string? Fallback { get; set; }
    public int Priority { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class ApprovalConditionDto
{
    public long? Id { get; set; }
    public string? ConditionName { get; set; }
    public string? Expression { get; set; }
    public long? TargetStepId { get; set; }
    public int Priority { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class ApprovalTaskDto
{
    public long Id { get; set; }
    public long InstanceId { get; set; }
    public long StepId { get; set; }
    public string? BusinessType { get; set; }
    public long BusinessId { get; set; }
    public string? StepName { get; set; }
    public string? TemplateName { get; set; }
    public long? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public string? AssigneeRole { get; set; }
    public int Status { get; set; }
    public string? Action { get; set; }
    public string? Comment { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Quotation-specific fields for display
    public string? RfqNo { get; set; }
    public string? CustomerName { get; set; }
    public string? Title { get; set; }

    // PmStepCycleTime-specific fields
    public long? PmStepId { get; set; }
    public long? TraceId { get; set; }
    public string? ProcessName { get; set; }
}
