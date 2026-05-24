using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>流水号规则</summary>
[Table("SysSequenceRule")]
public class SysSequenceRule
{
    [Key]
    public long Id { get; set; }

    /// <summary>业务模块类型（如 Opportunity, Quotation, Approval, PM）</summary>
    [Required]
    [StringLength(50)]
    public string ModuleType { get; set; } = string.Empty;

    /// <summary>模块名称描述</summary>
    [StringLength(100)]
    public string? ModuleName { get; set; }

    /// <summary>自定义前缀（如 OPP, QUO）</summary>
    [Required]
    [StringLength(20)]
    public string Prefix { get; set; } = string.Empty;

    /// <summary>当前流水号</summary>
    public long CurrentSequence { get; set; } = 0;

    /// <summary>流水号位数</summary>
    public int SequenceLength { get; set; } = 5;

    /// <summary>重置规则：Never=0, Daily=1, Monthly=2, Yearly=3</summary>
    public int ResetRule { get; set; } = 0;

    /// <summary>最后重置日期</summary>
    public DateTime? LastResetDate { get; set; }

    /// <summary>最后生成的编号</summary>
    [StringLength(100)]
    public string? LastGeneratedNo { get; set; }

    /// <summary>站点</summary>
    [StringLength(20)]
    public string? Site { get; set; }
    public string? App { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
