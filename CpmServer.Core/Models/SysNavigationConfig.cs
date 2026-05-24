using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>
/// 导航栏配置
/// </summary>
[Table("SysNavigationConfig")]
public class SysNavigationConfig
{
    [Key]
    public long Id { get; set; }

    /// <summary>导航项唯一标识，如 customer, quotation</summary>
    [Required]
    [StringLength(50)]
    public string NavCode { get; set; } = string.Empty;

    /// <summary>所属模块，如 rfq, pm, approval</summary>
    [Required]
    [StringLength(50)]
    public string ModuleCode { get; set; } = string.Empty;

    /// <summary>模块显示名（中文）</summary>
    [StringLength(100)]
    public string? ModuleLabel { get; set; }

    /// <summary>模块显示名（英文）</summary>
    [StringLength(100)]
    public string? ModuleLabelEn { get; set; }

    /// <summary>导航项显示名（中文）</summary>
    [StringLength(100)]
    public string? NavLabel { get; set; }

    /// <summary>导航项显示名（英文）</summary>
    [StringLength(100)]
    public string? NavLabelEn { get; set; }

    /// <summary>路由路径</summary>
    [Required]
    [StringLength(200)]
    public string RoutePath { get; set; } = string.Empty;

    /// <summary>图标名称</summary>
    [StringLength(50)]
    public string? IconName { get; set; }

    /// <summary>排序号</summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>是否显示</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>是否启用</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>站点隔离</summary>
    public string? Site { get; set; }
    public string? App { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
