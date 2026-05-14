using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>站点设置</summary>
[Table("SysSiteSettings")]
public class SysSiteSettings
{
    [Key]
    public long Id { get; set; }

    /// <summary>当前站点代码</summary>
    [Required]
    [StringLength(20)]
    public string Site { get; set; } = string.Empty;

    /// <summary>货币代码（如 CNY, USD, MYN, SGD）</summary>
    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = "CNY";

    /// <summary>公司名称/描述</summary>
    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}