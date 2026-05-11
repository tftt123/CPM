using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("MfgSubCategory")]
public class MfgSubCategory
{
    [Key]
    public long Id { get; set; }

    public long ProcessId { get; set; }

    public string? SubCategoryCode { get; set; }

    [Required]
    public string SubCategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Site { get; set; }

    public string? ToleranceGrade { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey("ProcessId")]
    public MfgProcess? Process { get; set; }

    public List<MfgEquipment> Equipments { get; set; } = new();
}
