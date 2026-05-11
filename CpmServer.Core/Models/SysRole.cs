using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysRole")]
public class SysRole
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string RoleCode { get; set; } = string.Empty;

    [Required]
    public string RoleName { get; set; } = string.Empty;

    public string? Site { get; set; }
}
