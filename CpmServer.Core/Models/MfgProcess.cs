using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("MfgProcess")]
public class MfgProcess
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    public string? ProcessCode { get; set; }

    [Required]
    public string ProcessName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Site { get; set; }

    public decimal? StdTimeMin { get; set; }

    public decimal? CostRate { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<MfgSubCategory> SubCategories { get; set; } = new();
}
