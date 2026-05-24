using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysRolePermission")]
public class SysRolePermission
{
    [Key]
    public long Id { get; set; }

    public long RoleId { get; set; }

    [Required]
    [StringLength(128)]
    public string PermissionCode { get; set; } = string.Empty;

    [StringLength(64)]
    public string? Site { get; set; }

    [StringLength(64)]
    public string? App { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("RoleId")]
    public SysRole? Role { get; set; }
}
