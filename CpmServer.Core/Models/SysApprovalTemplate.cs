using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysApprovalTemplate")]
public class SysApprovalTemplate
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string TemplateCode { get; set; } = string.Empty;

    [Required]
    public string TemplateName { get; set; } = string.Empty;

    public string ModuleType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public long? CreatedBy { get; set; }
    public string? Site { get; set; }
    public string? App { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<SysApprovalStep> Steps { get; set; } = new();
}
