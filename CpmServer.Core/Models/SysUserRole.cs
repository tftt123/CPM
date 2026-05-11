using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysUserRole")]
public class SysUserRole
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
}
