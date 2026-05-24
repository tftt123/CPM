using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysI18nMessage")]
public class SysI18nMessage
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string MessageKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ZhValue { get; set; }

    [MaxLength(500)]
    public string? EnValue { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Site { get; set; }
    public string? App { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
