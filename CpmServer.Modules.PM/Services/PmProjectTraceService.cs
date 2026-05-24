using CpmServer.Common;
using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using CpmServer.Modules.PM.Contracts;
using CpmServer.Modules.PM.DTOs;
using CpmServer.Services;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Modules.PM.Services;

public class PmProjectTraceService : IPmProjectTraceService
{
    private readonly CpmDbContext _db;
    private readonly IApprovalService _approvalService;
    private readonly ICurrentUser _currentUser;
    private readonly IGeneralizedCodeService _gc;

    public PmProjectTraceService(CpmDbContext db, IApprovalService approvalService, ICurrentUser currentUser, IGeneralizedCodeService gc)
    {
        _db = db;
        _approvalService = approvalService;
        _currentUser = currentUser;
        _gc = gc;
    }

    public async Task<PmProjectTraceListResponse> GetListAsync(string? keyword, int? status, int page, int pageSize)
    {
        var query = _db.ProjectTraces.AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(t => t.Site == _currentUser.Site);
        }

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
            .Select(t => new PmProjectTraceListItemDto
            {
                Id = t.Id,
                QuotationId = t.QuotationId,
                CustomerId = t.CustomerId,
                CustomerName = t.CustomerName,
                ProductId = t.ProductId,
                ProductCode = t.ProductCode,
                ProductName = t.ProductName,
                PlannedQty = t.PlannedQty,
                ProjectStartDate = t.ProjectStartDate,
                DisplayWeeks = t.DisplayWeeks,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        return new PmProjectTraceListResponse { Total = total, List = list };
    }

    public async Task<List<PmProjectTraceStepCycleTimeListItemDto>> GetAllStepsWithLatestCycleTimeAsync(string? keyword)
    {
        var query = _db.ProjectTraceSteps
            .Include(s => s.ProjectTrace)
                .ThenInclude(t => t!.Quotation)
            .Include(s => s.ActualCycleTimes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(s => s.Site == _currentUser.Site);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(s =>
                (s.ProjectTrace!.CustomerName != null && s.ProjectTrace.CustomerName.Contains(keyword)) ||
                (s.ProjectTrace.ProductCode != null && s.ProjectTrace.ProductCode.Contains(keyword)) ||
                (s.ProjectTrace.ProductName != null && s.ProjectTrace.ProductName.Contains(keyword)) ||
                (s.ProcessName != null && s.ProcessName.Contains(keyword)));
        }

        var stepIds = await query.Select(s => s.Id).ToListAsync();

        var pendingCounts = await _db.StepCycleTimeChangeRequests
            .Where(r => stepIds.Contains(r.StepId) && r.ApprovalStatus == 0)
            .GroupBy(r => r.StepId)
            .Select(g => new { StepId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StepId, x => x.Count);

        var steps = await query
            .OrderByDescending(s => s.ProjectTrace!.Quotation!.QuotationNo)
            .ThenBy(s => s.StepOrder)
            .ToListAsync();

        return steps.Select(s =>
        {
            var latest = s.ActualCycleTimes
                .Where(a => a.Status == 0)
                .OrderByDescending(a => a.RecordDate)
                .FirstOrDefault();
            return new PmProjectTraceStepCycleTimeListItemDto
            {
                TraceId = s.ProjectTraceId,
                QuotationNo = s.ProjectTrace!.Quotation?.QuotationNo,
                CustomerName = s.ProjectTrace.CustomerName ?? string.Empty,
                ProductCode = s.ProjectTrace.ProductCode ?? string.Empty,
                ProductName = s.ProjectTrace.ProductName,
                StepId = s.Id,
                StepOrder = s.StepOrder,
                ProcessName = s.ProcessName,
                PersonInCharge = s.PersonInCharge,
                CycleTime = s.CycleTime,
                LatestActualCycleTime = latest?.ActualCycleTime,
                LatestRecordDate = latest?.RecordDate,
                PendingRequestCount = pendingCounts.GetValueOrDefault(s.Id)
            };
        }).ToList();
    }

    public async Task<long> SubmitCycleTimeChangeRequestAsync(
        long stepId,
        long traceId,
        string submitterId,
        string submitterName,
        List<PmStepCycleTimeChangeDetailDto> changes)
    {
        if (!long.TryParse(submitterId, out var submitterIdLong))
            throw new BusinessException("Invalid submitter ID");

        var step = await _db.ProjectTraceSteps
            .Include(s => s.ActualCycleTimes)
            .FirstOrDefaultAsync(s => s.Id == stepId);

        if (step == null) throw new BusinessException("Step not found");

        var request = new PmStepCycleTimeChangeRequest
        {
            StepId = stepId,
            TraceId = traceId,
            SubmitterId = submitterId,
            SubmitterName = submitterName,
            SubmittedAt = DateTime.UtcNow,
            ApprovalStatus = 0,
            Site = _currentUser.Site,
            Details = changes.Select(c => new PmStepCycleTimeChangeDetail
            {
                ChangeType = c.ChangeType,
                TargetRecordId = c.TargetRecordId,
                RecordDate = c.RecordDate,
                ActualCycleTime = c.ActualCycleTime,
                Remarks = c.Remarks,
                CreatedAt = DateTime.UtcNow,
                Site = _currentUser.Site
            }).ToList(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.StepCycleTimeChangeRequests.Add(request);
            await _db.SaveChangesAsync();

            var submitter = await _db.Users.FindAsync(submitterIdLong);
            var site = submitter?.Site;

            var instance = await _approvalService.StartApprovalAsync(
                "PmStepCycleTime", request.Id, "PmStepCycleTime", site, submitterIdLong);
            request.ApprovalInstanceId = instance.Id;
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            return request.Id;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("未找到匹配的审批模板"))
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("未找到匹配的审批模板，请检查 PmStepCycleTime 模块的审批模板配置是否正确。");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ExecuteApprovedChangeRequestAsync(long requestId)
    {
        var request = await _db.StepCycleTimeChangeRequests
            .Include(r => r.Details)
            .Include(r => r.Step)
                .ThenInclude(s => s!.ActualCycleTimes)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null) throw new BusinessException("Request not found");
        if (request.Step == null) throw new BusinessException("Step not found");

        var step = request.Step;

        foreach (var detail in request.Details)
        {
            switch (detail.ChangeType)
            {
                case 0: // 新增
                    {
                        // 将所有有效记录标记为过期
                        var activeRecords = step.ActualCycleTimes.Where(a => a.Status == 0).ToList();
                        foreach (var r in activeRecords)
                        {
                            r.Status = 1;
                            r.UpdatedAt = DateTime.UtcNow;
                        }
                        // 插入新记录，状态=有效
                        step.ActualCycleTimes.Add(new PmProjectTraceStepActualCycleTime
                        {
                            ProjectTraceStepId = step.Id,
                            RecordDate = detail.RecordDate,
                            ActualCycleTime = detail.ActualCycleTime,
                            Remarks = detail.Remarks,
                            Status = 0,
                            Site = step.Site,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                    break;

                case 1: // 修改
                    {
                        if (!detail.TargetRecordId.HasValue) continue;
                        var oldRecord = step.ActualCycleTimes.FirstOrDefault(a => a.Id == detail.TargetRecordId.Value);
                        if (oldRecord == null) continue;

                        // 原记录变过期
                        oldRecord.Status = 1;
                        oldRecord.UpdatedAt = DateTime.UtcNow;

                        // 将所有有效记录标记为过期（确保只有一个有效）
                        var activeRecords = step.ActualCycleTimes.Where(a => a.Status == 0 && a.Id != oldRecord.Id).ToList();
                        foreach (var r in activeRecords)
                        {
                            r.Status = 1;
                            r.UpdatedAt = DateTime.UtcNow;
                        }

                        // 插入修改后的记录，状态=有效
                        step.ActualCycleTimes.Add(new PmProjectTraceStepActualCycleTime
                        {
                            ProjectTraceStepId = step.Id,
                            RecordDate = detail.RecordDate,
                            ActualCycleTime = detail.ActualCycleTime,
                            Remarks = detail.Remarks,
                            Status = 0,
                            Site = step.Site,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                    break;

                case 2: // 删除
                    {
                        if (!detail.TargetRecordId.HasValue) continue;
                        var record = step.ActualCycleTimes.FirstOrDefault(a => a.Id == detail.TargetRecordId.Value);
                        if (record == null) continue;

                        record.Status = 2; // 作废
                        record.UpdatedAt = DateTime.UtcNow;
                    }
                    break;
            }
        }

        request.ApprovalStatus = 1;
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task RejectChangeRequestAsync(long requestId, string? remarks)
    {
        var request = await _db.StepCycleTimeChangeRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null) throw new BusinessException("Request not found");

        request.ApprovalStatus = 2;
        request.Remarks = remarks;
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<PmStepCycleTimeChangeRequestDto?> GetChangeRequestDetailAsync(long requestId)
    {
        var request = await _db.StepCycleTimeChangeRequests
            .Include(r => r.Details)
            .Include(r => r.Step)
                .ThenInclude(s => s!.ProjectTrace)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null) return null;

        return new PmStepCycleTimeChangeRequestDto
        {
            Id = request.Id,
            StepId = request.StepId,
            TraceId = request.TraceId,
            SubmitterName = request.SubmitterName,
            SubmittedAt = request.SubmittedAt,
            ApprovalStatus = request.ApprovalStatus,
            Remarks = request.Remarks,
            ProcessName = request.Step?.ProcessName,
            CustomerName = request.Step?.ProjectTrace?.CustomerName,
            ProductCode = request.Step?.ProjectTrace?.ProductCode,
            ProductName = request.Step?.ProjectTrace?.ProductName,
            CycleTime = request.Step?.CycleTime,
            Details = request.Details.Select(d => new PmStepCycleTimeChangeDetailItemDto
            {
                ChangeType = d.ChangeType,
                TargetRecordId = d.TargetRecordId,
                RecordDate = d.RecordDate,
                ActualCycleTime = d.ActualCycleTime,
                Remarks = d.Remarks
            }).ToList()
        };
    }

    public async Task<PmProjectTraceDetailDto?> GetDetailAsync(long id)
    {
        var query = _db.ProjectTraces.AsQueryable();
        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(t => t.Site == _currentUser.Site);
        }

        var trace = await query
            .Include(t => t.Steps.OrderBy(s => s.StepOrder))
                .ThenInclude(s => s.ActualCycleTimes.OrderBy(a => a.RecordDate))
            .Include(t => t.Customer)
            .Include(t => t.Product)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trace == null) return null;

        return new PmProjectTraceDetailDto
        {
            Id = trace.Id,
            QuotationId = trace.QuotationId,
            CustomerId = trace.CustomerId,
            CustomerName = trace.CustomerName,
            ProductId = trace.ProductId,
            ProductCode = trace.ProductCode,
            ProductName = trace.ProductName,
            PlannedQty = trace.PlannedQty,
            ProjectStartDate = trace.ProjectStartDate,
            DisplayWeeks = trace.DisplayWeeks,
            Status = trace.Status,
            CreatedAt = trace.CreatedAt,
            UpdatedAt = trace.UpdatedAt,
            Steps = trace.Steps.Select(s => new PmProjectTraceStepDto
            {
                Id = s.Id,
                ProjectTraceId = s.ProjectTraceId,
                StepOrder = s.StepOrder,
                ProcessName = s.ProcessName,
                PersonInCharge = s.PersonInCharge,
                CycleTime = s.CycleTime,
                SettingDays = s.SettingDays,
                EstimatedHours = s.EstimatedHours,
                Remarks = s.Remarks,
                PlanDurationDays = s.PlanDurationDays,
                PlanStartDate = s.PlanStartDate,
                PlanEndDate = s.PlanEndDate,
                ActualStartDate = s.ActualStartDate,
                ActualForecastStartDate = s.ActualForecastStartDate,
                ActualDurationDays = s.ActualDurationDays,
                ActualPlanDurationDays = s.ActualPlanDurationDays,
                ActualEndDate = s.ActualEndDate,
                ActualCycleTimes = s.ActualCycleTimes.Select(a => new PmProjectTraceStepActualCycleTimeDto
                {
                    Id = a.Id,
                    ProjectTraceStepId = a.ProjectTraceStepId,
                    RecordDate = a.RecordDate,
                    ActualCycleTime = a.ActualCycleTime,
                    Remarks = a.Remarks,
                    Status = a.Status
                }).ToList()
            }).ToList()
        };
    }

    public async Task<long> CreateAsync(PmProjectTraceCreateRequest dto)
    {
        await _gc.ValidateAsync("PM_TRACE_STATUS", dto.Status.ToString(), _currentUser.Site, _currentUser.App ?? "cpm");

        var entity = new PmProjectTrace
        {
            QuotationId = dto.QuotationId,
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            ProductId = dto.ProductId,
            ProductCode = dto.ProductCode,
            ProductName = dto.ProductName,
            PlannedQty = dto.PlannedQty,
            ProjectStartDate = dto.ProjectStartDate,
            DisplayWeeks = dto.DisplayWeeks,
            Status = dto.Status,
            Site = _currentUser.Site,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Steps = dto.Steps.Select((s, i) => new PmProjectTraceStep
            {
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
                Site = _currentUser.Site,
                ActualCycleTimes = s.ActualCycleTimes.Select(a => new PmProjectTraceStepActualCycleTime
                {
                    RecordDate = a.RecordDate,
                    ActualCycleTime = a.ActualCycleTime,
                    Remarks = a.Remarks,
                    Site = _currentUser.Site,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList()
            }).ToList()
        };

        _db.ProjectTraces.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(long id, PmProjectTraceUpdateRequest dto)
    {
        var query = _db.ProjectTraces.AsQueryable();
        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(t => t.Site == _currentUser.Site);
        }

        var trace = await query
            .Include(t => t.Steps)
                .ThenInclude(s => s.ActualCycleTimes)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trace == null) throw new BusinessException("Record not found");

        await _gc.ValidateAsync("PM_TRACE_STATUS", dto.Status.ToString(), trace.Site, _currentUser.App ?? "cpm");

        trace.CustomerName = dto.CustomerName;
        trace.ProductCode = dto.ProductCode;
        trace.ProductName = dto.ProductName;
        trace.PlannedQty = dto.PlannedQty;
        trace.ProjectStartDate = dto.ProjectStartDate;
        trace.DisplayWeeks = dto.DisplayWeeks;
        trace.Status = dto.Status;
        trace.UpdatedAt = DateTime.UtcNow;

        var existingSteps = trace.Steps.ToDictionary(s => s.Id);
        var dtoStepIds = dto.Steps.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToHashSet();

        var stepsToRemove = trace.Steps.Where(s => !dtoStepIds.Contains(s.Id)).ToList();
        foreach (var step in stepsToRemove)
        {
            trace.Steps.Remove(step);
            _db.ProjectTraceSteps.Remove(step);
        }

        for (int i = 0; i < dto.Steps.Count; i++)
        {
            var stepDto = dto.Steps[i];
            if (stepDto.Id.HasValue && existingSteps.TryGetValue(stepDto.Id.Value, out var existingStep))
            {
                existingStep.StepOrder = i + 1;
                existingStep.ProcessName = stepDto.ProcessName;
                existingStep.PersonInCharge = stepDto.PersonInCharge;
                existingStep.CycleTime = stepDto.CycleTime;
                existingStep.SettingDays = stepDto.SettingDays;
                existingStep.EstimatedHours = stepDto.EstimatedHours;
                existingStep.Remarks = stepDto.Remarks;
                existingStep.PlanDurationDays = stepDto.PlanDurationDays;
                existingStep.PlanStartDate = stepDto.PlanStartDate;
                existingStep.PlanEndDate = stepDto.PlanStartDate.HasValue && stepDto.PlanDurationDays.HasValue
                    ? stepDto.PlanStartDate.Value.AddDays(stepDto.PlanDurationDays.Value)
                    : null;
                existingStep.ActualStartDate = stepDto.ActualStartDate;
                existingStep.ActualForecastStartDate = stepDto.ActualForecastStartDate;
                existingStep.ActualDurationDays = stepDto.ActualDurationDays;
                existingStep.ActualPlanDurationDays = stepDto.ActualPlanDurationDays;
                existingStep.ActualEndDate = stepDto.ActualEndDate;
            }
            else
            {
                var newStep = new PmProjectTraceStep
                {
                    ProjectTraceId = trace.Id,
                    StepOrder = i + 1,
                    ProcessName = stepDto.ProcessName,
                    PersonInCharge = stepDto.PersonInCharge,
                    CycleTime = stepDto.CycleTime,
                    SettingDays = stepDto.SettingDays,
                    EstimatedHours = stepDto.EstimatedHours,
                    Remarks = stepDto.Remarks,
                    PlanDurationDays = stepDto.PlanDurationDays,
                    PlanStartDate = stepDto.PlanStartDate,
                    PlanEndDate = stepDto.PlanStartDate.HasValue && stepDto.PlanDurationDays.HasValue
                        ? stepDto.PlanStartDate.Value.AddDays(stepDto.PlanDurationDays.Value)
                        : null,
                    ActualStartDate = stepDto.ActualStartDate,
                    ActualForecastStartDate = stepDto.ActualForecastStartDate,
                    ActualDurationDays = stepDto.ActualDurationDays,
                    ActualPlanDurationDays = stepDto.ActualPlanDurationDays,
                    ActualEndDate = stepDto.ActualEndDate,
                    Site = _currentUser.Site,
                    ActualCycleTimes = new List<PmProjectTraceStepActualCycleTime>()
                };
                trace.Steps.Add(newStep);
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var query = _db.ProjectTraces.AsQueryable();
        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(t => t.Site == _currentUser.Site);
        }

        var trace = await query
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trace == null) throw new BusinessException("Record not found");

        _db.ProjectTraceSteps.RemoveRange(trace.Steps);
        _db.ProjectTraces.Remove(trace);
        await _db.SaveChangesAsync();
    }
}
