using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysFieldControl")]
public class SysFieldControl
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string ModuleCode { get; set; } = string.Empty;

    [Required]
    public string PageCode { get; set; } = string.Empty;

    [Required]
    public string FieldCode { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;
    public bool IsRequired { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public string? Site { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
