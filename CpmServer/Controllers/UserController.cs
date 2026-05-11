using CpmServer.Common;
using CpmServer.DTOs.User;
using CpmServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CpmServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("list")]
    public async Task<ApiResult<PagedResult<UserDto>>> List(
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null)
    {
        var result = await _userService.GetListAsync(pageNum, pageSize, keyword);
        return ApiResult<PagedResult<UserDto>>.Success(result);
    }

    [HttpGet("{id}")]
    public async Task<ApiResult<UserDto?>> GetById(long id)
    {
        var result = await _userService.GetByIdAsync(id);
        return ApiResult<UserDto?>.Success(result);
    }

    [HttpPost]
    public async Task<ApiResult<long>> Add([FromBody] UserCreateDto dto)
    {
        var id = await _userService.CreateAsync(dto);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("{id}")]
    public async Task<ApiResult> Update(long id, [FromBody] UserUpdateDto dto)
    {
        await _userService.UpdateAsync(id, dto);
        return ApiResult.Success();
    }

    [HttpDelete("{id}")]
    public async Task<ApiResult> Delete(long id)
    {
        await _userService.DeleteAsync(id);
        return ApiResult.Success();
    }

    [HttpPost("{id}/reset-password")]
    public async Task<ApiResult> ResetPassword(long id, [FromBody] ResetPasswordRequest dto)
    {
        await _userService.ResetPasswordAsync(id, dto.NewPassword);
        return ApiResult.Success();
    }

    /// <summary>获取当前登录用户信息（无需 ADMIN 权限）</summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ApiResult<UserDto?>> GetProfile()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdStr, out var userId))
            return ApiResult<UserDto?>.Error("未登录");
        var result = await _userService.GetByIdAsync(userId);
        return ApiResult<UserDto?>.Success(result);
    }

    /// <summary>更新当前登录用户资料（无需 ADMIN 权限）</summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ApiResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdStr, out var userId))
            return ApiResult.Error("未登录");
        await _userService.UpdateProfileAsync(userId, dto);
        return ApiResult.Success();
    }

    /// <summary>修改当前登录用户密码</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ApiResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdStr, out var userId))
            return ApiResult.Error("未登录");
        await _userService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
        return ApiResult.Success();
    }
}

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
