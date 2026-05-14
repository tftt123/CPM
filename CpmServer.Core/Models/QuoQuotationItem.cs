using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("QuoQuotationItem")]
public class QuoQuotationItem
{
    [Key]
    public long Id { get; set; }

    public long QuotationId { get; set; }
    public long? ProductId { get; set; }
    public int Qty { get; set; } = 1;

    [Column(TypeName = "decimal(18,4)")]
    public decimal? UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? LineAmount { get; set; }

    public string? ProcessType { get; set; }
    public string? EquipmentType { get; set; }
    public string? Equipment { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? CycleTime { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? HourlyRate { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? Cost { get; set; }

    public bool IsProcessRow { get; set; }

    [ForeignKey("QuotationId")]
    public QuoQuotation? Quotation { get; set; }

    public string? Site { get; set; }

    [ForeignKey("ProductId")]
    public CrmProduct? Product { get; set; }
}
