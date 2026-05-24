using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>
/// 工序实际节拍历史记录 - 允许多次变更
/// </summary>
[Table("PmProjectTraceStepActualCycleTime")]
public class PmProjectTraceStepActualCycleTime
{
    [Key]
    public long Id { get; set; }

    public long ProjectTraceStepId { get; set; }

    /// <summary>记录日期</summary>
    public DateTime RecordDate { get; set; }

    /// <summary>实际节拍(Sec)</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? ActualCycleTime { get; set; }

    /// <summary>备注</summary>
    public string? Remarks { get; set; }

    /// <summary>状态: 0=有效, 1=过期, 2=作废</summary>
    public int Status { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public string? Site { get; set; }

    [ForeignKey("ProjectTraceStepId")]
    public PmProjectTraceStep? Step { get; set; }
}
