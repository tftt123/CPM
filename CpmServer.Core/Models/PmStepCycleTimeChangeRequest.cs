using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>工序实际节拍变更申请</summary>
[Table("PmStepCycleTimeChangeRequest")]
public class PmStepCycleTimeChangeRequest
{
    [Key]
    public long Id { get; set; }

    public long StepId { get; set; }
    public long TraceId { get; set; }

    public string? SubmitterId { get; set; }
    public string? SubmitterName { get; set; }
    public DateTime SubmittedAt { get; set; }

    /// <summary>审批状态: 0=待审批, 1=已通过, 2=已驳回</summary>
    public int ApprovalStatus { get; set; } = 0;

    /// <summary>审批实例ID</summary>
    public long? ApprovalInstanceId { get; set; }

    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public string? Site { get; set; }

    public List<PmStepCycleTimeChangeDetail> Details { get; set; } = new();

    [ForeignKey("StepId")]
    public PmProjectTraceStep? Step { get; set; }
}
