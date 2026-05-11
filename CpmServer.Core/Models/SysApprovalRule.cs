using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>
/// 审批人解析规则 —— 支持动态推导审批人
/// </summary>
[Table("SysApprovalRule")]
public class SysApprovalRule
{
    [Key]
    public long Id { get; set; }

    public long StepId { get; set; }

    /// <summary>
    /// 规则类型：FIXED_ROLE / FIXED_USER / ORG_TREE / EXPRESSION
    /// </summary>
    [Required]
    public string RuleType { get; set; } = string.Empty;

    /// <summary>
    /// 规则表达式
    /// FIXED_ROLE: "SALES_MANAGER"
    /// FIXED_USER: "12345"
    /// ORG_TREE: "SUBMITTER_DEPT_MANAGER"
    /// EXPRESSION: "Site == 'NT01' ? ROLE:A : ROLE:B"
    /// </summary>
    public string? RuleValue { get; set; }

    /// <summary>
    /// 兜底策略（解析失败时使用）
    /// </summary>
    public string? Fallback { get; set; }

    /// <summary>
    /// 规则优先级，数值小的优先
    /// </summary>
    public int Priority { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    [ForeignKey("StepId")]
    public SysApprovalStep? Step { get; set; }
}
