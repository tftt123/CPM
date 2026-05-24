using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

/// <summary>
/// 产品跟踪主表 - 报价单审批通过后自动生成
/// </summary>
[Table("PmProjectTrace")]
public class PmProjectTrace
{
    [Key]
    public long Id { get; set; }

    /// <summary>关联报价单</summary>
    public long? QuotationId { get; set; }

    /// <summary>客户ID（自动带入）</summary>
    public long CustomerId { get; set; }

    /// <summary>客户名称（自动带入）</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>产品ID（自动带入）</summary>
    public long? ProductId { get; set; }

    /// <summary>零件号/产品编码（自动带入）</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>产品名称（自动带入）</summary>
    public string? ProductName { get; set; }

    /// <summary>计划投入数量（手工填写）</summary>
    public int? PlannedQty { get; set; }

    /// <summary>项目开始日期（手工填写）</summary>
    public DateTime? ProjectStartDate { get; set; }

    /// <summary>显示周数（手工填写）</summary>
    public int? DisplayWeeks { get; set; }

    /// <summary>状态: 0=草稿 1=进行中 2=已完成</summary>
    public int Status { get; set; } = 0;

    public string? Site { get; set; }
    public string? App { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<PmProjectTraceStep> Steps { get; set; } = new();

    [ForeignKey("QuotationId")]
    public QuoQuotation? Quotation { get; set; }

    [ForeignKey("CustomerId")]
    public CrmCustomer? Customer { get; set; }

    [ForeignKey("ProductId")]
    public CrmProduct? Product { get; set; }
}
