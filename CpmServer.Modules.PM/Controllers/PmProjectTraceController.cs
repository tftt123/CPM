using CpmServer.Common;
using CpmServer.Modules.PM.Contracts;
using CpmServer.Modules.PM.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CpmServer.Modules.PM.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class PmProjectTraceController : ControllerBase
{
    private readonly IPmProjectTraceService _pmService;

    public PmProjectTraceController(IPmProjectTraceService pmService)
    {
        _pmService = pmService;
    }

    [HttpGet]
    public async Task<ApiResult<object>> GetList(
        [FromQuery] string? keyword,
        [FromQuery] int? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _pmService.GetListAsync(keyword, status, page, pageSize);
        return ApiResult<object>.Success(new { total = result.Total, list = result.List });
    }

    [HttpGet("steps/actual-cycle-time")]
    public async Task<ApiResult<object>> GetAllStepsWithLatestCycleTime([FromQuery] string? keyword)
    {
        var list = await _pmService.GetAllStepsWithLatestCycleTimeAsync(keyword);
        return ApiResult<object>.Success(list);
    }

    [HttpPost("steps/actual-cycle-time/change-request")]
    [Authorize(Policy = "CanApproveCycleTime")]
    public async Task<ApiResult<long>> SubmitCycleTimeChangeRequest([FromBody] SubmitCycleTimeChangeRequest dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";
        var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? userId;
        var id = await _pmService.SubmitCycleTimeChangeRequestAsync(dto.StepId, dto.TraceId, userId, userName, dto.Changes);
        return ApiResult<long>.Success(id);
    }

    [HttpGet("{id:long}")]
    public async Task<ApiResult<object>> GetDetail(long id)
    {
        var trace = await _pmService.GetDetailAsync(id);
        if (trace == null) return ApiResult<object>.Error("Record not found");

        return ApiResult<object>.Success(new
        {
            trace.Id,
            trace.QuotationId,
            trace.CustomerId,
            trace.CustomerName,
            trace.ProductId,
            trace.ProductCode,
            trace.ProductName,
            trace.PlannedQty,
            trace.ProjectStartDate,
            trace.DisplayWeeks,
            trace.Status,
            trace.CreatedAt,
            trace.UpdatedAt,
            Steps = trace.Steps.Select(s => new
            {
                s.Id,
                s.ProjectTraceId,
                s.StepOrder,
                s.ProcessName,
                s.PersonInCharge,
                s.CycleTime,
                s.SettingDays,
                s.EstimatedHours,
                s.Remarks,
                s.PlanDurationDays,
                s.PlanStartDate,
                s.PlanEndDate,
                s.ActualStartDate,
                s.ActualForecastStartDate,
                s.ActualDurationDays,
                s.ActualPlanDurationDays,
                s.ActualEndDate,
                ActualCycleTimes = s.ActualCycleTimes.Select(a => new
                {
                    a.Id,
                    a.ProjectTraceStepId,
                    a.RecordDate,
                    a.ActualCycleTime,
                    a.Remarks,
                    a.Status
                })
            })
        });
    }

    [HttpPost]
    public async Task<ApiResult<long>> Create([FromBody] PmProjectTraceCreateRequest dto)
    {
        var id = await _pmService.CreateAsync(dto);
        return ApiResult<long>.Success(id);
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] PmProjectTraceUpdateRequest dto)
    {
        await _pmService.UpdateAsync(id, dto);
        return ApiResult.Success();
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResult> Delete(long id)
    {
        await _pmService.DeleteAsync(id);
        return ApiResult.Success();
    }
}
