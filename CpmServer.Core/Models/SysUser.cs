using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysUser")]
public class SysUser
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? RealName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public long? DeptId { get; set; }
    public string? Site { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>所属部门</summary>
    public SysDept? Dept { get; set; }
}
