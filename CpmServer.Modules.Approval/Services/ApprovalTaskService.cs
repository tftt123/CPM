using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using CpmServer.Modules.Approval.DTOs;
using CpmServer.Services;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Modules.Approval.Services;

public class ApprovalTaskService : IApprovalTaskService
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ApprovalTaskService(CpmDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<ApprovalTaskDto>> GetPendingTasksAsync(long userId)
    {
        var user = await _db.Users
            .Include(u => u.Dept)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var userRoleIds = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var userRoleNames = await _db.Roles
            .Where(r => userRoleIds.Contains(r.Id) && (r.Site == _currentUser.Site || string.IsNullOrEmpty(r.Site)))
            .Select(r => r.RoleName)
            .ToListAsync();

        var currentApp = _currentUser.App ?? "cpm";
        var currentSite = _currentUser.Site;

        var tasks = await _db.ApprovalInstanceTasks
            .Include(t => t.Instance)
            .ThenInclude(i => i!.Template)
            .ThenInclude(t => t!.Steps)
            .Include(t => t.Assignee)
            .Where(t => t.Status == 0 &&
                (t.App == currentApp || string.IsNullOrEmpty(t.App)) &&
                (t.Site == currentSite || string.IsNullOrEmpty(t.Site)) &&
                (t.AssigneeId == userId || (t.AssigneeRole != null && userRoleNames.Contains(t.AssigneeRole))))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var dtoList = tasks.Select(t => MapTaskToDto(t)).ToList();

        // Enrich quotation tasks with detail fields
        var quotationTasks = dtoList.Where(t => t.BusinessType == "Quotation" && t.BusinessId > 0).ToList();
        if (quotationTasks.Count > 0)
        {
            var quotationIds = quotationTasks.Select(t => t.BusinessId).Distinct().ToList();
            var quotations = await _db.Quotations
                .Include(q => q.Customer)
                .Where(q => quotationIds.Contains(q.Id))
                .ToDictionaryAsync(q => q.Id, q => q);

            foreach (var task in quotationTasks)
            {
                if (quotations.TryGetValue(task.BusinessId, out var q))
                {
                    task.RfqNo = q.RfqNo;
                    task.CustomerName = q.Customer?.CustomerName;
                    task.Title = q.Title;
                }
            }
        }

        // Enrich PmStepCycleTime tasks with detail fields
        var cycleTimeTasks = dtoList.Where(t => t.BusinessType == "PmStepCycleTime" && t.BusinessId > 0).ToList();
        if (cycleTimeTasks.Count > 0)
        {
            var requestIds = cycleTimeTasks.Select(t => t.BusinessId).Distinct().ToList();
            var requests = await _db.StepCycleTimeChangeRequests
                .Include(r => r.Step)
                    .ThenInclude(s => s!.ProjectTrace)
                        .ThenInclude(t => t!.Quotation)
                .Where(r => requestIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r);

            foreach (var task in cycleTimeTasks)
            {
                if (requests.TryGetValue(task.BusinessId, out var req))
                {
                    task.PmStepId = req.StepId;
                    task.TraceId = req.TraceId;
                    task.ProcessName = req.Step?.ProcessName;
                    task.CustomerName = req.Step?.ProjectTrace?.CustomerName;
                    task.RfqNo = req.Step?.ProjectTrace?.Quotation?.RfqNo;
                    task.Title = req.Step?.ProcessName;
                }
            }
        }

        return dtoList;
    }

    public async Task<List<ApprovalTaskDto>> GetInstanceTasksAsync(long instanceId)
    {
        var tasks = await _db.ApprovalInstanceTasks
            .Include(t => t.Instance)
            .ThenInclude(i => i!.Template)
            .Include(t => t.Assignee)
            .Where(t => t.InstanceId == instanceId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(t => MapTaskToDto(t)).ToList();
    }

    public async Task<List<ApprovalRecordDto>> GetApprovalRecordsAsync(long instanceId)
    {
        return await _db.ApprovalRecords
            .Where(r => r.InstanceId == instanceId)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new ApprovalRecordDto
            {
                StepName = r.StepName,
                ApproverName = r.ApproverName,
                Action = r.Action,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<ApprovalStepDto>> GetApprovalStepsWithStatusAsync(long instanceId)
    {
        var instance = await _db.ApprovalInstances
            .Include(i => i.Template)
            .ThenInclude(t => t!.Steps)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance?.Template == null) return new List<ApprovalStepDto>();

        var records = await _db.ApprovalRecords
            .Where(r => r.InstanceId == instanceId)
            .ToListAsync();

        return instance.Template.Steps
            .Where(s => s.IsActive == true)
            .OrderBy(s => s.StepOrder)
            .Select(s => new ApprovalStepDto
            {
                Id = s.Id,
                StepName = s.StepName,
                StepOrder = s.StepOrder,
                StepType = s.StepType,
                StepMode = s.StepMode,
                ApproverRole = s.ApproverRole,
                ApproverUserId = s.ApproverUserId,
                Status = s.Id == instance.CurrentStepId ? 0 : (records.Any(r => r.StepId == s.Id) ? 1 : -1)
            })
            .ToList();
    }

    public async Task<bool> CanUserApproveAsync(long instanceId, long userId)
    {
        var instance = await _db.ApprovalInstances
            .Include(i => i.Template)
            .ThenInclude(t => t!.Steps)
            .Include(i => i.Tasks)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null || instance.Status != 0) return false;

        var currentStep = instance.Template?.Steps
            .FirstOrDefault(s => s.Id == instance.CurrentStepId);

        if (currentStep == null) return false;

        var pendingTasks = instance.Tasks.Where(t => t.StepId == currentStep.Id && t.Status == 0).ToList();
        if (pendingTasks.Any())
        {
            var directTask = pendingTasks.FirstOrDefault(t => t.AssigneeId == userId);
            if (directTask != null) return true;

            var userRoleIds = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var userRoleNames = await _db.Roles
                .Where(r => userRoleIds.Contains(r.Id) && (r.Site == _currentUser.Site || string.IsNullOrEmpty(r.Site)))
                .Select(r => r.RoleName)
                .ToListAsync();

            return pendingTasks.Any(t => t.AssigneeRole != null && userRoleNames.Contains(t.AssigneeRole));
        }

        if (currentStep.ApproverUserId.HasValue)
        {
            return currentStep.ApproverUserId.Value == userId;
        }

        if (!string.IsNullOrEmpty(currentStep.ApproverRole))
        {
            var userRoleIds = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var roleNames = await _db.Roles
                .Where(r => userRoleIds.Contains(r.Id) && (r.Site == _currentUser.Site || string.IsNullOrEmpty(r.Site)))
                .Select(r => r.RoleName)
                .ToListAsync();

            return roleNames.Contains(currentStep.ApproverRole);
        }

        return false;
    }

    public async Task<SysApprovalStep?> GetCurrentStepAsync(long instanceId)
    {
        var instance = await _db.ApprovalInstances
            .Include(i => i.Template)
            .ThenInclude(t => t!.Steps)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null || instance.CurrentStepId == null) return null;

        return instance.Template?.Steps
            .FirstOrDefault(s => s.Id == instance.CurrentStepId);
    }

    private static ApprovalTaskDto MapTaskToDto(SysApprovalInstanceTask t)
    {
        return new ApprovalTaskDto
        {
            Id = t.Id,
            InstanceId = t.InstanceId,
            StepId = t.StepId,
            BusinessType = t.Instance?.BusinessType,
            BusinessId = t.Instance?.BusinessId ?? 0,
            StepName = t.Instance?.Template?.Steps.FirstOrDefault(s => s.Id == t.StepId)?.StepName,
            TemplateName = t.Instance?.Template?.TemplateName,
            AssigneeId = t.AssigneeId,
            AssigneeName = t.Assignee?.RealName ?? t.Assignee?.Username,
            AssigneeRole = t.AssigneeRole,
            Status = t.Status,
            Action = t.Action,
            Comment = t.Comment,
            DueDate = t.DueDate,
            CreatedAt = t.CreatedAt,
            CompletedAt = t.CompletedAt
        };
    }
}
