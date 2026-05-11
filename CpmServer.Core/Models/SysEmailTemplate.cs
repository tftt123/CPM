using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysEmailTemplate")]
public class SysEmailTemplate
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string TemplateCode { get; set; } = string.Empty;

    [Required]
    public string TemplateName { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
