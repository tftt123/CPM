using CpmServer.Common;
using CpmServer.Modules.Quotation.DTOs;
using CpmServer.Modules.Quotation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CpmServer.Modules.Quotation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileUploadController : ControllerBase
{
    private readonly IFileUploadService _fileService;

    public FileUploadController(IFileUploadService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("upload")]
    public async Task<ApiResult<FileRecordDto>> Upload(IFormFile file, [FromForm] string moduleType, [FromForm] long? businessId)
    {
        var result = await _fileService.UploadAsync(file, moduleType, businessId);
        return ApiResult<FileRecordDto>.Success(result);
    }

    [HttpGet("files")]
    public async Task<ApiResult<List<FileRecordDto>>> GetFiles([FromQuery] string moduleType, [FromQuery] long businessId)
    {
        var result = await _fileService.GetFilesAsync(moduleType, businessId);
        return ApiResult<List<FileRecordDto>>.Success(result);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        await _fileService.DeleteFileAsync(id);
        return ApiResult.Success();
    }
}
