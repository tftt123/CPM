using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PmProjectTraceController : ControllerBase
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public PmProjectTraceController(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? keyword,
        [FromQuery] int? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.ProjectTraces
            .Include(t => t.Steps)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(t =>
                t.CustomerName.Contains(keyword) ||
                t.ProductCode.Contains(keyword) ||
                t.ProductName!.Contains(keyword));
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        var total = await query.CountAsync();
        var list = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { total, list });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetDetail(long id)
    {
        var trace = await _db.ProjectTraces
            .Include(t => t.Steps.OrderBy(s => s.StepOrder))
            .Include(t => t.Customer)
            .Include(t => t.Product)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trace == null) return NotFound();
        return Ok(trace);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PmProjectTrace dto)
    {
        dto.CreatedAt = DateTime.Now;
        dto.UpdatedAt = DateTime.Now;
        foreach (var step in dto.Steps)
        {
            // 自动计算计划结束日期
            if (step.PlanStartDate.HasValue && step.PlanDurationDays.HasValue)
            {
                step.PlanEndDate = step.PlanStartDate.Value.AddDays(step.PlanDurationDays.Value);
            }
        }
        _db.ProjectTraces.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] PmProjectTrace dto)
    {
        var trace = await _db.ProjectTraces
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trace == null) return NotFound();

        trace.CustomerName = dto.CustomerName;
        trace.ProductCode = dto.ProductCode;
        trace.ProductName = dto.ProductName;
        trace.PlannedQty = dto.PlannedQty;
        trace.ProjectStartDate = dto.ProjectStartDate;
        trace.DisplayWeeks = dto.DisplayWeeks;
        trace.Status = dto.Status;
        trace.UpdatedAt = DateTime.Now;

        // 更新工序明细
        _db.ProjectTraceSteps.RemoveRange(trace.Steps);
        trace.Steps = dto.Steps.Select((s, i) => new PmProjectTraceStep
        {
            ProjectTraceId = trace.Id,
            StepOrder = i + 1,
            ProcessName = s.ProcessName,
            PersonInCharge = s.PersonInCharge,
            CycleTime = s.CycleTime,
            SettingDays = s.SettingDays,
            EstimatedHours = s.EstimatedHours,
            Remarks = s.Remarks,
            PlanDurationDays = s.PlanDurationDays,
            PlanStartDate = s.PlanStartDate,
            PlanEndDate = s.PlanStartDate.HasValue && s.PlanDurationDays.HasValue
                ? s.PlanStartDate.Value.AddDays(s.PlanDurationDays.Value)
                : null,
            ActualStartDate = s.ActualStartDate,
            ActualForecastStartDate = s.ActualForecastStartDate,
            ActualDurationDays = s.ActualDurationDays,
            ActualPlanDurationDays = s.ActualPlanDurationDays,
            ActualEndDate = s.ActualEndDate,
        }).ToList();

        await _db.SaveChangesAsync();
        return Ok(trace);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var trace = await _db.ProjectTraces
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trace == null) return NotFound();

        _db.ProjectTraceSteps.RemoveRange(trace.Steps);
        _db.ProjectTraces.Remove(trace);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
