using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Modules.PM.Services;

public class PmStepCycleTimeBusinessStatusUpdater : IBusinessStatusUpdater
{
    private readonly CpmDbContext _db;

    public PmStepCycleTimeBusinessStatusUpdater(CpmDbContext db)
    {
        _db = db;
    }

    public bool Supports(string businessType) => businessType == "PmStepCycleTime";

    public async Task UpdateStatusAsync(long businessId, int approvalStatus, long? currentStepId, decimal? reviewCost = null)
    {
        if (approvalStatus == 1)
        {
            await ExecuteApprovedAsync(businessId);
        }
        else if (approvalStatus == 2)
        {
            await RejectAsync(businessId);
        }
    }

    private async Task ExecuteApprovedAsync(long requestId)
    {
        var request = await _db.StepCycleTimeChangeRequests
            .Include(r => r.Details)
            .Include(r => r.Step)
                .ThenInclude(s => s!.ActualCycleTimes)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null || request.Step == null) return;

        var step = request.Step;

        foreach (var detail in request.Details)
        {
            switch (detail.ChangeType)
            {
                case 0: // 新增
                    {
                        var activeRecords = step.ActualCycleTimes.Where(a => a.Status == 0).ToList();
                        foreach (var r in activeRecords)
                        {
                            r.Status = 1;
                            r.UpdatedAt = DateTime.Now;
                        }
                        step.ActualCycleTimes.Add(new PmProjectTraceStepActualCycleTime
                        {
                            ProjectTraceStepId = step.Id,
                            RecordDate = detail.RecordDate,
                            ActualCycleTime = detail.ActualCycleTime,
                            Remarks = detail.Remarks,
                            Status = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }
                    break;

                case 1: // 修改
                    {
                        if (!detail.TargetRecordId.HasValue) continue;
                        var oldRecord = step.ActualCycleTimes.FirstOrDefault(a => a.Id == detail.TargetRecordId.Value);
                        if (oldRecord == null) continue;

                        oldRecord.Status = 1;
                        oldRecord.UpdatedAt = DateTime.Now;

                        var activeRecords = step.ActualCycleTimes.Where(a => a.Status == 0 && a.Id != oldRecord.Id).ToList();
                        foreach (var r in activeRecords)
                        {
                            r.Status = 1;
                            r.UpdatedAt = DateTime.Now;
                        }

                        step.ActualCycleTimes.Add(new PmProjectTraceStepActualCycleTime
                        {
                            ProjectTraceStepId = step.Id,
                            RecordDate = detail.RecordDate,
                            ActualCycleTime = detail.ActualCycleTime,
                            Remarks = detail.Remarks,
                            Status = 0,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }
                    break;

                case 2: // 删除
                    {
                        if (!detail.TargetRecordId.HasValue) continue;
                        var record = step.ActualCycleTimes.FirstOrDefault(a => a.Id == detail.TargetRecordId.Value);
                        if (record == null) continue;

                        record.Status = 2;
                        record.UpdatedAt = DateTime.Now;
                    }
                    break;
            }
        }

        request.ApprovalStatus = 1;
        request.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task RejectAsync(long requestId)
    {
        var request = await _db.StepCycleTimeChangeRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null) return;

        request.ApprovalStatus = 2;
        request.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }
}
