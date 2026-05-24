using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysFileRecord")]
public class SysFileRecord
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    public string OriginalName { get; set; } = string.Empty;

    [Required]
    public string FilePath { get; set; } = string.Empty;

    public string? FileType { get; set; }

    public long? FileSize { get; set; }

    public string? ModuleType { get; set; }

    public long? BusinessId { get; set; }

    public string? Site { get; set; }
    public string? App { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
