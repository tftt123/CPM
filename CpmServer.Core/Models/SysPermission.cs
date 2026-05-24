using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysPermission")]
public class SysPermission
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(128)]
    public string PermissionCode { get; set; } = string.Empty;

    [StringLength(256)]
    public string? PermissionName { get; set; }

    [StringLength(256)]
    public string? Description { get; set; }

    [StringLength(64)]
    public string? Module { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
