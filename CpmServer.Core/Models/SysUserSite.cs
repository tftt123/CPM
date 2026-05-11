using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysUserSite")]
public class SysUserSite
{
    public long UserId { get; set; }
    public string Site { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public SysUser? User { get; set; }
}
