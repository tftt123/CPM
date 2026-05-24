using CpmServer.Authorization;
using CpmServer.Common;
using CpmServer.DTOs.Role;
using CpmServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CpmServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.CanManageSystem)]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("list")]
    [AllowAnonymous]
    public async Task<ApiResult<List<RoleDto>>> List([FromQuery] string? site = null)
    {
        var result = await _roleService.GetListAsync(site);
        return ApiResult<List<RoleDto>>.Success(result);
    }

    [HttpGet("{id}")]
    public async Task<ApiResult<RoleDto?>> GetById(long id)
    {
        var result = await _roleService.GetByIdAsync(id);
        return ApiResult<RoleDto?>.Success(result);
    }

    [HttpPost]
    public async Task<ApiResult<long>> Add([FromBody] RoleCreateDto dto)
    {
        var id = await _roleService.CreateAsync(dto);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("{id}")]
    public async Task<ApiResult> Update(long id, [FromBody] RoleUpdateDto dto)
    {
        await _roleService.UpdateAsync(id, dto);
        return ApiResult.Success();
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        await _roleService.DeleteAsync(id);
        return ApiResult.Success();
    }

    [HttpPut("{id}/permissions")]
    public async Task<ApiResult> AssignPermissions(long id, [FromBody] List<string> permissionCodes)
    {
        await _roleService.UpdateRolePermissionsAsync(id, permissionCodes);
        return ApiResult.Success();
    }

    [HttpGet("permissions/all")]
    [AllowAnonymous]
    public Task<ApiResult<List<string>>> GetAllPermissions()
    {
        return Task.FromResult(ApiResult<List<string>>.Success(Permissions.All.ToList()));
    }
}
