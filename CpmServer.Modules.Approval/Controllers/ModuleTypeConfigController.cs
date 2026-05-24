using CpmServer.Common;
using CpmServer.Constants;
using CpmServer.Models;
using CpmServer.Modules.Approval.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CpmServer.Modules.Approval.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModuleTypeConfigController : ControllerBase
{
    private readonly IModuleTypeConfigService _service;

    public ModuleTypeConfigController(IModuleTypeConfigService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ApiResult<List<SysModuleTypeConfig>>> GetList([FromQuery] bool all = false)
    {
        var list = await _service.GetListAsync(!all);
        return ApiResult<List<SysModuleTypeConfig>>.Success(list);
    }

    [HttpGet("system-types")]
    public ApiResult<List<string>> GetSystemTypes()
    {
        return ApiResult<List<string>>.Success(SystemModuleTypes.All.ToList());
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResult<SysModuleTypeConfig?>> GetById(long id)
    {
        var entity = await _service.GetByIdAsync(id);
        return ApiResult<SysModuleTypeConfig?>.Success(entity);
    }

    [HttpPost]
    public async Task<ApiResult<long>> Create([FromBody] SysModuleTypeConfig entity)
    {
        var id = await _service.CreateAsync(entity);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] SysModuleTypeConfig entity)
    {
        await _service.UpdateAsync(id, entity);
        return ApiResult.Success();
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResult> Delete(long id)
    {
        await _service.DeleteAsync(id);
        return ApiResult.Success();
    }
}
