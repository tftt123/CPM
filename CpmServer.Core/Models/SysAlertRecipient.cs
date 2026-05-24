using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>
/// 报警收件人配置
/// </summary>
[Table("SysAlertRecipient")]
public class SysAlertRecipient
{
    [Key]
    public long Id { get; set; }

    /// <summary>报警类型，如 CYCLE_TIME_EXCEEDED</summary>
    [Required]
    [StringLength(50)]
    public string AlertType { get; set; } = string.Empty;

    /// <summary>收件人类型：EMAIL / ROLE</summary>
    [Required]
    [StringLength(20)]
    public string RecipientType { get; set; } = string.Empty;

    /// <summary>收件人值：邮箱地址或角色名</summary>
    [Required]
    [StringLength(200)]
    public string RecipientValue { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public string? Site { get; set; }
}
