namespace CpmServer.Modules.Approval.DTOs;

public class ApprovalActionDto
{
    public long QuotationId { get; set; }
    public string Action { get; set; } = string.Empty; // APPROVE / REJECT / TRANSFER
    public string? Comment { get; set; }
    public long? TransferToUserId { get; set; }
    public decimal? ReviewCost { get; set; }
}

public class ApprovalRecordDto
{
    public long Id { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string? ApproverName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApprovalStepDto
{
    public long Id { get; set; }
    public string StepName { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public string StepType { get; set; } = string.Empty;
    public string StepMode { get; set; } = string.Empty;
    public string? ApproverRole { get; set; }
    public string? ApproverRoleName { get; set; }
    public long? ApproverUserId { get; set; }
    public string? ApproverUserName { get; set; }
    public bool CanReject { get; set; }
    public bool CanTransfer { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public int Status { get; set; }
}
