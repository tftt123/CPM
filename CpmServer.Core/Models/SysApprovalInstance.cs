using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysApprovalInstance")]
public class SysApprovalInstance
{
    [Key]
    public long Id { get; set; }

    public long TemplateId { get; set; }
    public string BusinessType { get; set; } = string.Empty;
    public long BusinessId { get; set; }
    public long? CurrentStepId { get; set; }
    public int CurrentStepOrder { get; set; }

    /// <summary>
    /// 0=进行中 1=完成 2=驳回
    /// </summary>
    public int Status { get; set; } = 0;

    public string? Site { get; set; }
    public string? App { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 流程变量 JSON（Phase 1 新增）
    /// </summary>
    public string? Variables { get; set; }

    /// <summary>
    /// 发起人Id
    /// </summary>
    public long? SubmitterId { get; set; }

    [ForeignKey("TemplateId")]
    public SysApprovalTemplate? Template { get; set; }

    public List<SysApprovalRecord> Records { get; set; } = new();

    /// <summary>
    /// 待办任务列表（Phase 1 新增）
    /// </summary>
    public List<SysApprovalInstanceTask> Tasks { get; set; } = new();
}
