using CpmServer.Common;
using CpmServer.Modules.Approval.Contracts;
using CpmServer.Modules.Approval.DTOs;
using CpmServer.Modules.Quotation.Contracts;
using CpmServer.Modules.Quotation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CpmServer.Modules.Quotation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuotationController : ControllerBase
{
    private readonly IQuotationService _quotationService;
    private readonly IApprovalService _approvalService;

    public QuotationController(IQuotationService quotationService, IApprovalService approvalService)
    {
        _quotationService = quotationService;
        _approvalService = approvalService;
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("用户未登录或令牌无效");
        }
        return userId;
    }

    #region 商机管理

    [HttpGet("opportunities")]
    public async Task<ApiResult<PagedResult<OpportunityDto>>> GetOpportunityList(
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] string? stage = null)
    {
        var result = await _quotationService.GetOpportunityListAsync(pageNum, pageSize, keyword, stage);
        return ApiResult<PagedResult<OpportunityDto>>.Success(result);
    }

    [HttpGet("opportunities/{id}")]
    public async Task<ApiResult<OpportunityDto?>> GetOpportunityById(long id)
    {
        var result = await _quotationService.GetOpportunityByIdAsync(id);
        return ApiResult<OpportunityDto?>.Success(result);
    }

    [HttpPost("opportunities")]
    public async Task<ApiResult<long>> CreateOpportunity([FromBody] OpportunityCreateDto dto)
    {
        var userId = GetCurrentUserId();
        var id = await _quotationService.CreateOpportunityAsync(dto, userId);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("opportunities/{id}")]
    public async Task<ApiResult> UpdateOpportunity(long id, [FromBody] OpportunityCreateDto dto)
    {
        await _quotationService.UpdateOpportunityAsync(id, dto);
        return ApiResult.Success();
    }

    [HttpPatch("opportunities/{id}/stage")]
    public async Task<ApiResult> UpdateOpportunityStage(long id, [FromBody] OpportunityStageUpdateDto dto)
    {
        await _quotationService.UpdateOpportunityStageAsync(id, dto.Stage);
        return ApiResult.Success();
    }

    [HttpDelete("opportunities/{id}")]
    public async Task<ApiResult> DeleteOpportunity(long id)
    {
        await _quotationService.DeleteOpportunityAsync(id);
        return ApiResult.Success();
    }

    #endregion

    #region 报价单管理

    [HttpGet("quotations")]
    public async Task<ApiResult<PagedResult<QuotationDto>>> GetQuotationList(
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] int? status = null)
    {
        var result = await _quotationService.GetQuotationListAsync(pageNum, pageSize, keyword, status);
        return ApiResult<PagedResult<QuotationDto>>.Success(result);
    }

    [HttpGet("quotations/{id}")]
    public async Task<ApiResult<QuotationDto?>> GetQuotationById(long id)
    {
        var result = await _quotationService.GetQuotationByIdAsync(id);
        return ApiResult<QuotationDto?>.Success(result);
    }

    [HttpPost("quotations")]
    public async Task<ApiResult<long>> CreateQuotation([FromBody] QuotationCreateDto dto)
    {
        var userId = GetCurrentUserId();
        var id = await _quotationService.CreateQuotationAsync(dto, userId);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("quotations/{id}")]
    public async Task<ApiResult> UpdateQuotation(long id, [FromBody] QuotationCreateDto dto)
    {
        await _quotationService.UpdateQuotationAsync(id, dto);
        return ApiResult.Success();
    }

    [HttpDelete("quotations/{id}")]
    public async Task<ApiResult> DeleteQuotation(long id)
    {
        await _quotationService.DeleteQuotationAsync(id);
        return ApiResult.Success();
    }

    #endregion

    #region 审批操作

    [HttpPost("quotations/{id}/submit")]
    public async Task<ApiResult> SubmitForApproval(long id)
    {
        var userId = GetCurrentUserId();
        await _quotationService.SubmitForApprovalAsync(id, userId);
        return ApiResult.Success();
    }

    [HttpPost("quotations/{id}/approve")]
    public async Task<ApiResult> ProcessApproval(long id, [FromBody] CpmServer.Modules.Quotation.DTOs.ApprovalActionDto dto)
    {
        var userId = GetCurrentUserId();
        await _quotationService.ProcessApprovalAsync(id, dto, userId);
        return ApiResult.Success();
    }

    [HttpGet("quotations/{id}/approval-records")]
    public async Task<ApiResult<List<ApprovalRecordDto>>> GetApprovalRecords(long id)
    {
        var result = await _quotationService.GetApprovalRecordsAsync(id);
        return ApiResult<List<ApprovalRecordDto>>.Success(result);
    }

    [HttpGet("quotations/{id}/approval-steps")]
    public async Task<ApiResult<List<ApprovalStepDto>>> GetApprovalSteps(long id)
    {
        var result = await _quotationService.GetApprovalStepsAsync(id);
        return ApiResult<List<ApprovalStepDto>>.Success(result);
    }

    [HttpGet("quotations/{id}/can-approve")]
    public async Task<ApiResult<bool>> CanApprove(long id)
    {
        var userId = GetCurrentUserId();
        var instance = await _approvalService.GetInstanceAsync("Quotation", id);
        if (instance == null || instance.Status != 0)
            return ApiResult<bool>.Success(false);
        var canApprove = await _approvalService.CanUserApproveAsync(instance.Id, userId);
        return ApiResult<bool>.Success(canApprove);
    }

    /// <summary>
    /// 审批预测：预览该报价单的完整审批路线（不创建实例）
    /// </summary>
    [HttpGet("quotations/{id}/approval-forecast")]
    public async Task<ApiResult<List<ApprovalForecastStepDto>>> GetApprovalForecast(long id)
    {
        var userId = GetCurrentUserId();
        var result = await _approvalService.ForecastApprovalAsync("QUOTATION", "Quotation", id, userId);
        return ApiResult<List<ApprovalForecastStepDto>>.Success(result);
    }

    #endregion
}
