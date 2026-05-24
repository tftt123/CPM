using CpmServer.Common;
using CpmServer.DTOs.MfgProcess;

namespace CpmServer.Services;

public interface IMfgProcessService
{
    // Cascade options
    Task<List<string>> GetCategoryListAsync();
    Task<List<MfgCascadeOption>> GetProcessOptionsAsync(string? category = null);
    Task<List<MfgCascadeOption>> GetSubCategoryOptionsAsync(long processId);

    // Process
    Task<List<MfgProcessDto>> GetProcessesAsync(string? category);
    Task<long> CreateProcessAsync(MfgProcessCreateDto dto);
    Task UpdateProcessAsync(long id, MfgProcessCreateDto dto);
    Task DeleteProcessAsync(long id);

    // SubCategory
    Task<List<MfgSubCategoryDto>> GetSubCategoriesAsync(long? processId);
    Task<long> CreateSubCategoryAsync(MfgSubCategoryCreateDto dto);
    Task UpdateSubCategoryAsync(long id, MfgSubCategoryCreateDto dto);
    Task DeleteSubCategoryAsync(long id);

    // Equipment
    Task<List<MfgEquipmentDto>> GetEquipmentsAsync(long? subCategoryId);
    Task<long> CreateEquipmentAsync(MfgEquipmentCreateDto dto);
    Task UpdateEquipmentAsync(long id, MfgEquipmentCreateDto dto);
    Task DeleteEquipmentAsync(long id);

    // Flat records for maintenance page
    Task<List<MfgProcessRecordDto>> GetFlatRecordsAsync(string? category, string? keyword);

    // Import from Excel
    Task<(int imported, int skipped, List<string> errors)> ImportFromExcelAsync(Stream excelStream);
}
