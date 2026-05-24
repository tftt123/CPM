using CpmServer.Common;
using CpmServer.Models;
using CpmServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CpmServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FieldControlController : ControllerBase
{
    private readonly IFieldControlService _service;

    public FieldControlController(IFieldControlService service)
    {
        _service = service;
    }

    [HttpGet("modules")]
    public ApiResult<List<string>> GetModules()
    {
        return ApiResult<List<string>>.Success(_service.GetModules());
    }

    [HttpGet("pages")]
    public ApiResult<List<string>> GetPages([FromQuery] string moduleCode)
    {
        return ApiResult<List<string>>.Success(_service.GetPages(moduleCode));
    }

    [HttpGet]
    public async Task<ApiResult<List<SysFieldControl>>> GetFields([FromQuery] string moduleCode, [FromQuery] string pageCode)
    {
        var fields = await _service.GetFieldsAsync(moduleCode, pageCode);
        return ApiResult<List<SysFieldControl>>.Success(fields);
    }

    [HttpPost("init")]
    public async Task<ApiResult<List<SysFieldControl>>> InitFields([FromBody] InitFieldRequest request)
    {
        var fields = await _service.InitFieldsAsync(request.ModuleCode, request.PageCode, request.Fields);
        return ApiResult<List<SysFieldControl>>.Success(fields);
    }

    [HttpPut("batch")]
    public async Task<ApiResult> UpdateBatch([FromBody] List<SysFieldControl> fields)
    {
        await _service.UpdateBatchAsync(fields);
        return ApiResult.Success();
    }
}

public class InitFieldRequest
{
    public string ModuleCode { get; set; } = string.Empty;
    public string PageCode { get; set; } = string.Empty;
    public List<FieldInitDto> Fields { get; set; } = new();
}
