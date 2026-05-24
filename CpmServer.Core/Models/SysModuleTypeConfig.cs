using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>业务模块类型配置 - 用于审批模板匹配</summary>
[Table("SysModuleTypeConfig")]
public class SysModuleTypeConfig
{
    [Key]
    public long Id { get; set; }

    /// <summary>模块类型编码（唯一，如 Quotation, PmStepCycleTime）</summary>
    [Required]
    public string ModuleType { get; set; } = string.Empty;

    /// <summary>模块显示名称</summary>
    public string? ModuleName { get; set; }

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>是否启用</summary>
    public bool IsActive { get; set; } = true;

    [StringLength(64)]
    public string? Site { get; set; }

    [StringLength(64)]
    public string? App { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
