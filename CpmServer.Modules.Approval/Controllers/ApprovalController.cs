using CpmServer.Common;
using CpmServer.Modules.Approval.Contracts;
using CpmServer.Modules.Approval.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CpmServer.Modules.Approval.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    #region 审批模板管理（ADMIN 权限）

    [HttpGet("templates")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ApiResult<List<ApprovalTemplateDto>>> GetTemplates([FromQuery] string? moduleType)
    {
        var result = await _approvalService.GetTemplatesAsync(moduleType);
        return ApiResult<List<ApprovalTemplateDto>>.Success(result);
    }

    [HttpGet("templates/{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ApiResult<ApprovalTemplateDto?>> GetTemplateById(long id)
    {
        var result = await _approvalService.GetTemplateByIdAsync(id);
        return ApiResult<ApprovalTemplateDto?>.Success(result);
    }

    [HttpPost("templates")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ApiResult<long>> CreateTemplate([FromBody] ApprovalTemplateDto dto)
    {
        var id = await _approvalService.CreateTemplateAsync(dto);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("templates/{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ApiResult> UpdateTemplate(long id, [FromBody] ApprovalTemplateDto dto)
    {
        await _approvalService.UpdateTemplateAsync(id, dto);
        return ApiResult.Success();
    }

    [HttpDelete("templates/{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ApiResult> DeleteTemplate(long id)
    {
        await _approvalService.DeleteTemplateAsync(id);
        return ApiResult.Success();
    }

    #endregion

    #region 待办任务查询（所有登录用户）

    /// <summary>
    /// 获取当前用户的待办任务列表
    /// </summary>
    [HttpGet("my-tasks")]
    public async Task<ApiResult<List<ApprovalTaskDto>>> GetMyPendingTasks()
    {
        var userId = GetCurrentUserId();
        var result = await _approvalService.GetPendingTasksAsync(userId);
        return ApiResult<List<ApprovalTaskDto>>.Success(result);
    }

    /// <summary>
    /// 获取某个审批实例的任务列表
    /// </summary>
    [HttpGet("instances/{instanceId}/tasks")]
    public async Task<ApiResult<List<ApprovalTaskDto>>> GetInstanceTasks(long instanceId)
    {
        var result = await _approvalService.GetInstanceTasksAsync(instanceId);
        return ApiResult<List<ApprovalTaskDto>>.Success(result);
    }

    #endregion

    #region 审批预测（Forecast）

    /// <summary>
    /// 审批预测：预览某业务单据的完整审批路线（不创建实例）
    /// </summary>
    [HttpGet("forecast")]
    public async Task<ApiResult<List<ApprovalForecastStepDto>>> ForecastApproval(
        [FromQuery] string moduleType,
        [FromQuery] string businessType,
        [FromQuery] long businessId)
    {
        var userId = GetCurrentUserId();
        var result = await _approvalService.ForecastApprovalAsync(moduleType, businessType, businessId, userId);
        return ApiResult<List<ApprovalForecastStepDto>>.Success(result);
    }

    #endregion

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("无法获取当前用户ID");
        }
        return userId;
    }
}
