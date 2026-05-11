using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("CrmCustomer")]
public class CrmCustomer
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string CustomerCode { get; set; } = string.Empty;

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    public string? Industry { get; set; }
    public string? Region { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsFromQad { get; set; }
    public string? QadCustomerCode { get; set; }
    public string? Site { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
