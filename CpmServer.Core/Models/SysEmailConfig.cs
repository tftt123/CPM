using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysEmailConfig")]
public class SysEmailConfig
{
    [Key]
    public long Id { get; set; }

    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string? Site { get; set; }
}
