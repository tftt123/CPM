using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("QuoQuotation")]
public class QuoQuotation
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string QuotationNo { get; set; } = string.Empty;

    public string RfqNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    public long OpportunityId { get; set; }
    public long CustomerId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalAmount { get; set; }

    public long? CurrentStepId { get; set; }

    public int Status { get; set; } = 0;

    public long CreatedBy { get; set; }
    public string? Site { get; set; }
    public string? App { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey("OpportunityId")]
    public QuoOpportunity? Opportunity { get; set; }

    [ForeignKey("CustomerId")]
    public CrmCustomer? Customer { get; set; }

    public List<QuoQuotationItem> Items { get; set; } = new();
}
