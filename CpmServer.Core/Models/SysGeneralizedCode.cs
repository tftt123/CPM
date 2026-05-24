using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>
/// 通用代码（Generalized Code）——参考 QAD ERP 36.2.13
/// 一个表维护所有系统下拉选项，通过 Domain 区分不同用途
/// </summary>
[Table("SysGeneralizedCode")]
public class SysGeneralizedCode
{
    [Key]
    public long Id { get; set; }

    /// <summary>代码域/类别，如 OPP_STAGE, QUO_STATUS, INDUSTRY</summary>
    [Required]
    [StringLength(64)]
    public string Domain { get; set; } = string.Empty;

    /// <summary>代码值，如 NEW, QUALIFIED, AUTOMOTIVE</summary>
    [Required]
    [StringLength(64)]
    public string Code { get; set; } = string.Empty;

    /// <summary>显示名（中文）</summary>
    [StringLength(128)]
    public string? Label { get; set; }

    /// <summary>显示名（英文）</summary>
    [StringLength(128)]
    public string? LabelEn { get; set; }

    /// <summary>排序号</summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>是否有效（软删除用）</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Element Plus tag 类型：success/warning/danger/info/primary</summary>
    [StringLength(32)]
    public string? TagType { get; set; }

    /// <summary>附加 JSON 属性（预留扩展）</summary>
    [StringLength(500)]
    public string? Attributes { get; set; }

    /// <summary>站点隔离</summary>
    [StringLength(64)]
    public string? Site { get; set; }

    /// <summary>应用隔离，如 cpm / sp / mes</summary>
    [StringLength(32)]
    public string? App { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
