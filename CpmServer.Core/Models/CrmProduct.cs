using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("CrmProduct")]
public class CrmProduct
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string ProductCode { get; set; } = string.Empty;

    [Required]
    public string ProductName { get; set; } = string.Empty;

    public string? Material { get; set; }
    public string? SurfaceTreatment { get; set; }
    public string? Tolerance { get; set; }
    public string? DrawingNo { get; set; }
    public bool IsFromQad { get; set; }
    public string? QadItemCode { get; set; }
    public string? Site { get; set; }
    public string? App { get; set; }
}
