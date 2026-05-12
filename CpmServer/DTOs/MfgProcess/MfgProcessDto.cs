namespace CpmServer.DTOs.MfgProcess;

public class MfgProcessDto
{
    public long Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ProcessCode { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Owner { get; set; }
    public decimal? StdTimeMin { get; set; }
    public decimal? CostRate { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class MfgProcessCreateDto
{
    public string Category { get; set; } = string.Empty;
    public string? ProcessCode { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Owner { get; set; }
    public decimal? StdTimeMin { get; set; }
    public decimal? CostRate { get; set; }
    public int SortOrder { get; set; }
}

public class MfgSubCategoryDto
{
    public long Id { get; set; }
    public long ProcessId { get; set; }
    public string? Category { get; set; }
    public string? ProcessName { get; set; }
    public string? SubCategoryCode { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ToleranceGrade { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public List<MfgEquipmentDto> Equipments { get; set; } = new();
}

public class MfgSubCategoryCreateDto
{
    public long ProcessId { get; set; }
    public string? SubCategoryCode { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ToleranceGrade { get; set; }
    public int SortOrder { get; set; }
    public List<MfgEquipmentCreateDto> Equipments { get; set; } = new();
}

public class MfgEquipmentDto
{
    public long Id { get; set; }
    public long SubCategoryId { get; set; }
    public string? Category { get; set; }
    public string? ProcessName { get; set; }
    public string? SubCategoryName { get; set; }
    public string? EquipmentCode { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Model { get; set; }
    public string? Spec { get; set; }
    public string? Manufacturer { get; set; }
    public string? Owner { get; set; }
    public decimal? CostRate { get; set; }
    public bool IsActive { get; set; }
}

public class MfgEquipmentCreateDto
{
    public long SubCategoryId { get; set; }
    public string? EquipmentCode { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Model { get; set; }
    public string? Spec { get; set; }
    public string? Manufacturer { get; set; }
    public string? Owner { get; set; }
    public decimal? CostRate { get; set; }
}

// Flat record for maintenance page - one row per equipment with full path
public class MfgProcessRecordDto
{
    public long EquipmentId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string SubCategoryName { get; set; } = string.Empty;
    public string? EquipmentCode { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Model { get; set; }
    public string? Spec { get; set; }
    public string? Manufacturer { get; set; }
    public string? Owner { get; set; }
    public decimal? CostRate { get; set; }
    public bool IsActive { get; set; }
    public long ProcessId { get; set; }
    public long SubCategoryId { get; set; }
}

// Cascade dropdown options
public class MfgCascadeOption
{
    public long Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Owner { get; set; }
}
