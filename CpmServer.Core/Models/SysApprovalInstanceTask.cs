using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>
/// 审批实例任务 —— 每个步骤可生成多个待办任务（支持会签、角色审批）
/// </summary>
[Table("SysApprovalInstanceTask")]
public class SysApprovalInstanceTask
{
    [Key]
    public long Id { get; set; }

    public long InstanceId { get; set; }

    /// <summary>
    /// 对应模板步骤Id
    /// </summary>
    public long StepId { get; set; }

    /// <summary>
    /// 被指派人Id（精确到人）
    /// </summary>
    public long? AssigneeId { get; set; }

    /// <summary>
    /// 被指派的角色的待办（角色审批时未解析到具体人）
    /// </summary>
    public string? AssigneeRole { get; set; }

    /// <summary>
    /// 0=待办 1=已处理 2=转交 3=超时
    /// </summary>
    public int Status { get; set; } = 0;

    /// <summary>
    /// APPROVE / REJECT / TRANSFER
    /// </summary>
    public string? Action { get; set; }

    public string? Comment { get; set; }

    /// <summary>
    /// 截止期限
    /// </summary>
    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }

    [ForeignKey("InstanceId")]
    public SysApprovalInstance? Instance { get; set; }

    [ForeignKey("AssigneeId")]
    public SysUser? Assignee { get; set; }
}
