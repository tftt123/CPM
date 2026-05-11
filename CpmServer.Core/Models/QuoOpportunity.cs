using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("QuoOpportunity")]
public class QuoOpportunity
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string OpportunityNo { get; set; } = string.Empty;

    public long CustomerId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ExpectedAmount { get; set; }

    public DateTime? QuoteDeadline { get; set; }

    public string Stage { get; set; } = "NEW";

    public int Status { get; set; } = 0;

    public long OwnerId { get; set; }
    public string? Site { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey("CustomerId")]
    public CrmCustomer? Customer { get; set; }

    [ForeignKey("OwnerId")]
    public SysUser? Owner { get; set; }

    public List<QuoQuotation> Quotations { get; set; } = new();
}
