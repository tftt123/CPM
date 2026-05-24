using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>
/// 产品跟踪工序明细
/// </summary>
[Table("PmProjectTraceStep")]
public class PmProjectTraceStep
{
    [Key]
    public long Id { get; set; }

    public long ProjectTraceId { get; set; }

    /// <summary>工序序号</summary>
    public int StepOrder { get; set; }

    // === 工艺信息 ===
    /// <summary>工艺步骤名</summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>设备/机器</summary>
    public string? Equipment { get; set; }

    /// <summary>责任人</summary>
    public string? PersonInCharge { get; set; }

    // === 工艺路线汇总（底部表格，蓝色=手工填写）===
    /// <summary>节拍(Sec) - 手工</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? CycleTime { get; set; }

    /// <summary>设定(天) - 手工</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? SettingDays { get; set; }

    /// <summary>预计耗时 - 手工</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? EstimatedHours { get; set; }

    /// <summary>备注</summary>
    public string? Remarks { get; set; }

    // === 计划信息（上半部分）===
    /// <summary>计划持续天数 - 手工</summary>
    public int? PlanDurationDays { get; set; }

    /// <summary>计划开始日期 - 手工</summary>
    public DateTime? PlanStartDate { get; set; }

    /// <summary>计划结束日期 - 自动计算（开始+天数）</summary>
    public DateTime? PlanEndDate { get; set; }

    // === 实际信息（下半部分，全部蓝色=手工）===
    /// <summary>实际开始日期 - 手工</summary>
    public DateTime? ActualStartDate { get; set; }

    /// <summary>实际/预计开始日期 - 手工</summary>
    public DateTime? ActualForecastStartDate { get; set; }

    /// <summary>实际持续天数 - 手工</summary>
    public int? ActualDurationDays { get; set; }

    /// <summary>实际/计划持续天数 - 手工</summary>
    public int? ActualPlanDurationDays { get; set; }

    /// <summary>实际结束日期 - 手工</summary>
    public DateTime? ActualEndDate { get; set; }

    /// <summary>实际节拍历史记录</summary>
    public List<PmProjectTraceStepActualCycleTime> ActualCycleTimes { get; set; } = new();

    public string? Site { get; set; }

    [ForeignKey("ProjectTraceId")]
    public PmProjectTrace? ProjectTrace { get; set; }
}
