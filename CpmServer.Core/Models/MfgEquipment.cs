using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpmServer.Models;

[Table("MfgEquipment")]
public class MfgEquipment
{
    [Key]
    public long Id { get; set; }

    public long SubCategoryId { get; set; }

    public string? EquipmentCode { get; set; }

    [Required]
    public string EquipmentName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Site { get; set; }

    public string? Model { get; set; }

    public string? Spec { get; set; }

    public string? Manufacturer { get; set; }

    public string? Owner { get; set; }

    public decimal? CostRate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey("SubCategoryId")]
    public MfgSubCategory? SubCategory { get; set; }
}
