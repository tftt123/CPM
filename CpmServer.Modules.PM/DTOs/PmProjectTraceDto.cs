using System.ComponentModel.DataAnnotations;

namespace CpmServer.Modules.PM.DTOs;

public class PmProjectTraceStepActualCycleTimeDto
{
    public long? Id { get; set; }
    public long? ProjectTraceStepId { get; set; }
    public DateTime RecordDate { get; set; }
    public decimal? ActualCycleTime { get; set; }
    public string? Remarks { get; set; }
    public int Status { get; set; }
}

public class PmProjectTraceStepDto
{
    public long? Id { get; set; }
    public long? ProjectTraceId { get; set; }
    public int StepOrder { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string? PersonInCharge { get; set; }
    public decimal? CycleTime { get; set; }
    public decimal? SettingDays { get; set; }
    public decimal? EstimatedHours { get; set; }
    public string? Remarks { get; set; }
    public int? PlanDurationDays { get; set; }
    public DateTime? PlanStartDate { get; set; }
    public DateTime? PlanEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualForecastStartDate { get; set; }
    public int? ActualDurationDays { get; set; }
    public int? ActualPlanDurationDays { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public List<PmProjectTraceStepActualCycleTimeDto> ActualCycleTimes { get; set; } = new();
}

public class PmProjectTraceListItemDto
{
    public long Id { get; set; }
    public long? QuotationId { get; set; }
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public long? ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public int? PlannedQty { get; set; }
    public DateTime? ProjectStartDate { get; set; }
    public int? DisplayWeeks { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PmProjectTraceDetailDto : PmProjectTraceListItemDto
{
    public List<PmProjectTraceStepDto> Steps { get; set; } = new();
}

public class PmProjectTraceListResponse
{
    public int Total { get; set; }
    public List<PmProjectTraceListItemDto> List { get; set; } = new();
}

public class PmProjectTraceCreateRequest
{
    public long? QuotationId { get; set; }
    public long CustomerId { get; set; }
    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    public long? ProductId { get; set; }
    [Required]
    [StringLength(200)]
    public string ProductCode { get; set; } = string.Empty;
    [StringLength(500)]
    public string? ProductName { get; set; }
    [Range(1, int.MaxValue)]
    public int? PlannedQty { get; set; }
    public DateTime? ProjectStartDate { get; set; }
    public int? DisplayWeeks { get; set; }
    public int Status { get; set; } = 0;
    public List<PmProjectTraceStepDto> Steps { get; set; } = new();
}

public class PmProjectTraceUpdateRequest
{
    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    [Required]
    [StringLength(200)]
    public string ProductCode { get; set; } = string.Empty;
    [StringLength(500)]
    public string? ProductName { get; set; }
    [Range(1, int.MaxValue)]
    public int? PlannedQty { get; set; }
    public DateTime? ProjectStartDate { get; set; }
    public int? DisplayWeeks { get; set; }
    public int Status { get; set; }
    public List<PmProjectTraceStepDto> Steps { get; set; } = new();
}

public class PmProjectTraceStepCycleTimeListItemDto
{
    public long TraceId { get; set; }
    public string? QuotationNo { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public long StepId { get; set; }
    public int StepOrder { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string? PersonInCharge { get; set; }
    public decimal? CycleTime { get; set; }
    public decimal? LatestActualCycleTime { get; set; }
    public DateTime? LatestRecordDate { get; set; }
    public int PendingRequestCount { get; set; }
}

public class PmStepCycleTimeChangeDetailDto
{
    public int ChangeType { get; set; }
    public long? TargetRecordId { get; set; }
    public DateTime RecordDate { get; set; }
    public decimal? ActualCycleTime { get; set; }
    public string? Remarks { get; set; }
}

public class SubmitCycleTimeChangeRequest
{
    public long StepId { get; set; }
    public long TraceId { get; set; }
    public List<PmStepCycleTimeChangeDetailDto> Changes { get; set; } = new();
}

public class PmProjectTraceStepActualCycleTimeUpdateRequest
{
    public List<PmProjectTraceStepActualCycleTimeDto> ActualCycleTimes { get; set; } = new();
}

public class PmStepCycleTimeChangeRequestDto
{
    public long Id { get; set; }
    public long StepId { get; set; }
    public long TraceId { get; set; }
    public string? SubmitterName { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int ApprovalStatus { get; set; }
    public string? Remarks { get; set; }
    public string? ProcessName { get; set; }
    public string? CustomerName { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public decimal? CycleTime { get; set; }
    public List<PmStepCycleTimeChangeDetailItemDto> Details { get; set; } = new();
}

public class PmStepCycleTimeChangeDetailItemDto
{
    public int ChangeType { get; set; }
    public long? TargetRecordId { get; set; }
    public DateTime RecordDate { get; set; }
    public decimal? ActualCycleTime { get; set; }
    public string? Remarks { get; set; }
}
