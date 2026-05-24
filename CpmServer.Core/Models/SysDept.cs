using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysDept")]
public class SysDept
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string DeptName { get; set; } = string.Empty;

    public long ParentId { get; set; }

    /// <summary>部门主管用户Id</summary>
    public long? ManagerId { get; set; }

    public string? Site { get; set; }
    public string? App { get; set; }

    /// <summary>部门主管</summary>
    public SysUser? Manager { get; set; }
}
