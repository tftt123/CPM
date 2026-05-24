using CpmServer.Common;
using CpmServer.DTOs.MfgProcess;
using CpmServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CpmServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class MfgProcessController : ControllerBase
{
    private readonly IMfgProcessService _mfgService;

    public MfgProcessController(IMfgProcessService mfgService)
    {
        _mfgService = mfgService;
    }

    #region Cascade Options

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ApiResult<List<string>>> GetCategoryList()
    {
        var result = await _mfgService.GetCategoryListAsync();
        return ApiResult<List<string>>.Success(result);
    }

    [HttpGet("process-options")]
    [AllowAnonymous]
    public async Task<ApiResult<List<MfgCascadeOption>>> GetProcessOptions([FromQuery] string? category = null)
    {
        var result = await _mfgService.GetProcessOptionsAsync(category);
        return ApiResult<List<MfgCascadeOption>>.Success(result);
    }

    [HttpGet("subcategory-options")]
    [AllowAnonymous]
    public async Task<ApiResult<List<MfgCascadeOption>>> GetSubCategoryOptions([FromQuery] long processId)
    {
        var result = await _mfgService.GetSubCategoryOptionsAsync(processId);
        return ApiResult<List<MfgCascadeOption>>.Success(result);
    }

    #endregion

    #region Flat Records

    [HttpGet("records")]
    public async Task<ApiResult<List<MfgProcessRecordDto>>> GetFlatRecords(
        [FromQuery] string? category,
        [FromQuery] string? keyword)
    {
        var result = await _mfgService.GetFlatRecordsAsync(category, keyword);
        return ApiResult<List<MfgProcessRecordDto>>.Success(result);
    }

    #endregion

    #region Process

    [HttpGet("processes")]
    public async Task<ApiResult<List<MfgProcessDto>>> GetProcesses([FromQuery] string? category)
    {
        var result = await _mfgService.GetProcessesAsync(category);
        return ApiResult<List<MfgProcessDto>>.Success(result);
    }

    [HttpPost("processes")]
    public async Task<ApiResult<long>> CreateProcess([FromBody] MfgProcessCreateDto dto)
    {
        var id = await _mfgService.CreateProcessAsync(dto);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("processes/{id}")]
    public async Task<ApiResult> UpdateProcess(long id, [FromBody] MfgProcessCreateDto dto)
    {
        await _mfgService.UpdateProcessAsync(id, dto);
        return ApiResult.Success();
    }

    [HttpDelete("processes/{id}")]
    public async Task<ApiResult> DeleteProcess(long id)
    {
        await _mfgService.DeleteProcessAsync(id);
        return ApiResult.Success();
    }

    #endregion

    #region SubCategory

    [HttpGet("subcategories")]
    public async Task<ApiResult<List<MfgSubCategoryDto>>> GetSubCategories([FromQuery] long? processId)
    {
        var result = await _mfgService.GetSubCategoriesAsync(processId);
        return ApiResult<List<MfgSubCategoryDto>>.Success(result);
    }

    [HttpPost("subcategories")]
    public async Task<ApiResult<long>> CreateSubCategory([FromBody] MfgSubCategoryCreateDto dto)
    {
        var id = await _mfgService.CreateSubCategoryAsync(dto);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("subcategories/{id}")]
    public async Task<ApiResult> UpdateSubCategory(long id, [FromBody] MfgSubCategoryCreateDto dto)
    {
        await _mfgService.UpdateSubCategoryAsync(id, dto);
        return ApiResult.Success();
    }

    [HttpDelete("subcategories/{id}")]
    public async Task<ApiResult> DeleteSubCategory(long id)
    {
        await _mfgService.DeleteSubCategoryAsync(id);
        return ApiResult.Success();
    }

    #endregion

    #region Equipment

    [HttpGet("equipments")]
    public async Task<ApiResult<List<MfgEquipmentDto>>> GetEquipments([FromQuery] long? subCategoryId)
    {
        var result = await _mfgService.GetEquipmentsAsync(subCategoryId);
        return ApiResult<List<MfgEquipmentDto>>.Success(result);
    }

    [HttpPost("equipments")]
    public async Task<ApiResult<long>> CreateEquipment([FromBody] MfgEquipmentCreateDto dto)
    {
        var id = await _mfgService.CreateEquipmentAsync(dto);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("equipments/{id}")]
    public async Task<ApiResult> UpdateEquipment(long id, [FromBody] MfgEquipmentCreateDto dto)
    {
        await _mfgService.UpdateEquipmentAsync(id, dto);
        return ApiResult.Success();
    }

    [HttpDelete("equipments/{id}")]
    public async Task<ApiResult> DeleteEquipment(long id)
    {
        await _mfgService.DeleteEquipmentAsync(id);
        return ApiResult.Success();
    }

    #endregion

    #region Import

    [HttpPost("import")]
    public async Task<ApiResult> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ApiResult.Error("请上传Excel文件");

        using var stream = file.OpenReadStream();
        var (imported, skipped, errors) = await _mfgService.ImportFromExcelAsync(stream);

        var message = $"导入完成: 成功{imported}条, 跳过{skipped}条";
        if (errors.Count > 0)
            message += $", 失败{errors.Count}条";

        return ApiResult.Success(new { imported, skipped, errors, message });
    }

    #endregion
}
