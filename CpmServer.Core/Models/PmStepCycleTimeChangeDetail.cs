using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>工序实际节拍变更明细</summary>
[Table("PmStepCycleTimeChangeDetail")]
public class PmStepCycleTimeChangeDetail
{
    [Key]
    public long Id { get; set; }

    public long RequestId { get; set; }

    /// <summary>变更类型: 0=新增, 1=修改, 2=删除</summary>
    public int ChangeType { get; set; }

    /// <summary>目标记录ID（修改/删除时）</summary>
    public long? TargetRecordId { get; set; }

    public DateTime RecordDate { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? ActualCycleTime { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey("RequestId")]
    public PmStepCycleTimeChangeRequest? Request { get; set; }
}
