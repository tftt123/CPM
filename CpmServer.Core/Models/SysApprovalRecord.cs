using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("SysApprovalRecord")]
public class SysApprovalRecord
{
    [Key]
    public long Id { get; set; }

    public long InstanceId { get; set; }
    public long? StepId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public long? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public string? Site { get; set; }
    public string? App { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey("InstanceId")]
    public SysApprovalInstance? Instance { get; set; }
}
