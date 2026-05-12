using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CpmServer.Modules.Approval.Services;

public class ApprovalActionService : IApprovalActionService
{
    private readonly CpmDbContext _db;
    private readonly ILogger<ApprovalActionService> _logger;
    private readonly ApprovalNotificationService _notificationService;
    private readonly ApprovalInstanceService _instanceService;
    private readonly IEnumerable<IBusinessVariableProvider> _variableProviders;
    private readonly IEnumerable<IBusinessStatusUpdater> _statusUpdaters;

    public ApprovalActionService(
        CpmDbContext db,
        ILogger<ApprovalActionService> logger,
        ApprovalNotificationService notificationService,
        ApprovalInstanceService instanceService,
        IEnumerable<IBusinessVariableProvider> variableProviders,
        IEnumerable<IBusinessStatusUpdater> statusUpdaters)
    {
        _db = db;
        _logger = logger;
        _notificationService = notificationService;
        _instanceService = instanceService;
        _variableProviders = variableProviders;
        _statusUpdaters = statusUpdaters;
    }

    public async Task ApproveAsync(long instanceId, long approverId, string comment, decimal? reviewCost = null)
    {
        var instance = await _db.ApprovalInstances
            .Include(i => i.Template)
            .ThenInclude(t => t!.Steps)
            .ThenInclude(s => s.Rules)
            .Include(i => i.Tasks)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null)
        {
            throw new InvalidOperationException("审批实例不存在");
        }

        if (instance.Status != 0)
        {
            throw new InvalidOperationException("审批流程已结束");
        }

        var currentStep = instance.Template?.Steps
            .FirstOrDefault(s => s.Id == instance.CurrentStepId);

        if (currentStep == null)
        {
            throw new InvalidOperationException("当前步骤不存在");
        }

        if (!await CanUserApproveAsync(instanceId, approverId))
        {
            throw new UnauthorizedAccessException("您没有权限审批此步骤");
        }

        var approver = await _db.Users.FindAsync(approverId);

        _db.ApprovalRecords.Add(new SysApprovalRecord
        {
            InstanceId = instanceId,
            StepId = currentStep.Id,
            StepName = currentStep.StepName,
            ApproverId = approverId,
            ApproverName = approver?.RealName ?? approver?.Username,
            Action = "APPROVE",
            Comment = comment,
            Site = instance.Site,
            CreatedAt = DateTime.UtcNow
        });

        var myTask = instance.Tasks
            .FirstOrDefault(t => t.StepId == currentStep.Id && t.Status == 0 &&
                (t.AssigneeId == approverId || t.AssigneeRole != null));
        if (myTask != null)
        {
            myTask.Status = 1;
            myTask.Action = "APPROVE";
            myTask.Comment = comment;
            myTask.CompletedAt = DateTime.UtcNow;
        }

        var pendingTasks = await _db.ApprovalInstanceTasks
            .Where(t => t.InstanceId == instanceId && t.StepId == currentStep.Id && t.Status == 0)
            .ToListAsync();

        bool shouldAdvance = currentStep.StepMode switch
        {
            "PARALLEL" => !pendingTasks.Any(t => t.Status == 0 && t.Id != myTask?.Id),
            "PARALLEL_ANY" => true,
            "CC" => true,
            _ => true
        };

        if (shouldAdvance && currentStep.StepMode == "PARALLEL_ANY")
        {
            foreach (var task in pendingTasks.Where(t => t.Status == 0 && t.Id != myTask?.Id))
            {
                task.Status = 1;
                task.Action = "SKIP";
                task.Comment = "或签模式，他人已审批";
                task.CompletedAt = DateTime.UtcNow;
            }
        }

        if (shouldAdvance)
        {
            var variables = instance.Variables != null
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(instance.Variables) ?? new Dictionary<string, string>()
                : new Dictionary<string, string>();

            var nextStep = await ResolveNextStepAsync(instance, currentStep, variables);

            if (nextStep != null)
            {
                instance.CurrentStepId = nextStep.Id;
                instance.CurrentStepOrder = nextStep.StepOrder;
                await _instanceService.CreateStepTasksAsync(instance, nextStep, instance.SubmitterId ?? 0, variables);

                if (nextStep.StepMode == "END")
                {
                    _logger.LogInformation("到达结束节点，流程自动完成: InstanceId={InstanceId}", instance.Id);
                }
            }
            else
            {
                instance.Status = 1;
                instance.CurrentStepId = null;
                instance.CompletedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();

        await UpdateBusinessStatusAsync(instance, reviewCost);

        if (instance.Status == 0 && shouldAdvance)
        {
            var variables = instance.Variables != null
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(instance.Variables) ?? new Dictionary<string, string>()
                : new Dictionary<string, string>();
            var nextStep = await ResolveNextStepAsync(instance, currentStep, variables);
            if (nextStep != null && nextStep.StepMode != "END")
            {
                await _notificationService.NotifyStepAsync(instance, nextStep, instance.BusinessType, instance.BusinessId);
            }
        }
        else if (instance.Status == 1)
        {
            await _notificationService.NotifyCompleteAsync(instance, instance.BusinessType, instance.BusinessId, true);
        }

        _logger.LogInformation("审批通过: InstanceId={InstanceId}, Step={StepName}, Approver={ApproverId}",
            instanceId, currentStep.StepName, approverId);
    }

    public async Task RejectAsync(long instanceId, long approverId, string comment)
    {
        var instance = await _db.ApprovalInstances
            .Include(i => i.Template)
            .ThenInclude(t => t!.Steps)
            .Include(i => i.Tasks)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null || instance.Status != 0)
        {
            throw new InvalidOperationException("审批实例不存在或已结束");
        }

        var currentStep = instance.Template?.Steps
            .FirstOrDefault(s => s.Id == instance.CurrentStepId);

        if (currentStep == null)
        {
            throw new InvalidOperationException("当前步骤不存在");
        }

        if (!currentStep.CanReject)
        {
            throw new InvalidOperationException("当前步骤不允许驳回");
        }

        if (!await CanUserApproveAsync(instanceId, approverId))
        {
            throw new UnauthorizedAccessException("您没有权限审批此步骤");
        }

        var approver = await _db.Users.FindAsync(approverId);

        _db.ApprovalRecords.Add(new SysApprovalRecord
        {
            InstanceId = instanceId,
            StepId = currentStep.Id,
            StepName = currentStep.StepName,
            ApproverId = approverId,
            ApproverName = approver?.RealName ?? approver?.Username,
            Action = "REJECT",
            Comment = comment,
            Site = instance.Site,
            CreatedAt = DateTime.UtcNow
        });

        foreach (var task in instance.Tasks.Where(t => t.StepId == currentStep.Id && t.Status == 0))
        {
            task.Status = 1;
            task.Action = "REJECT";
            task.Comment = comment;
            task.CompletedAt = DateTime.UtcNow;
        }

        var steps = instance.Template?.Steps
            .Where(s => s.IsActive == true)
            .OrderBy(s => s.StepOrder)
            .ToList() ?? new List<SysApprovalStep>();

        SysApprovalStep? targetStep = currentStep.RejectBehavior switch
        {
            "REJECT_TO_PREV" => steps
                .Where(s => s.StepOrder < currentStep.StepOrder)
                .OrderByDescending(s => s.StepOrder)
                .FirstOrDefault(),
            "REJECT_TO_STEP" => steps
                .FirstOrDefault(s => s.Id == currentStep.RejectTargetStepId),
            "REJECT_TO_START" => steps.FirstOrDefault(),
            "REJECT_TO_REQUESTOR" => steps
                .FirstOrDefault(s => s.StepMode == "START" || s.StepOrder == steps.Min(st => st.StepOrder)),
            _ => null
        };

        if (targetStep != null)
        {
            instance.CurrentStepId = targetStep.Id;
            instance.CurrentStepOrder = targetStep.StepOrder;
            instance.Status = 0;
            instance.CompletedAt = null;

            await _db.SaveChangesAsync();

            var variables = instance.Variables != null
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(instance.Variables) ?? new Dictionary<string, string>()
                : new Dictionary<string, string>();
            await _instanceService.CreateStepTasksAsync(instance, targetStep, instance.SubmitterId ?? 0, variables);

            if (targetStep.StepMode == "START" || currentStep.RejectBehavior == "REJECT_TO_REQUESTOR")
            {
                var nextAfterStart = steps
                    .Where(s => s.StepOrder > targetStep.StepOrder)
                    .OrderBy(s => s.StepOrder)
                    .FirstOrDefault();
                if (nextAfterStart != null)
                {
                    instance.CurrentStepId = nextAfterStart.Id;
                    instance.CurrentStepOrder = nextAfterStart.StepOrder;
                    await _db.SaveChangesAsync();
                    await _instanceService.CreateStepTasksAsync(instance, nextAfterStart, instance.SubmitterId ?? 0, variables);
                    if (nextAfterStart.StepMode != "END")
                    {
                        await _notificationService.NotifyStepAsync(instance, nextAfterStart, instance.BusinessType, instance.BusinessId);
                    }
                    _logger.LogInformation("退回发起人后自动推进: InstanceId={InstanceId}, NextStep={NextStep}",
                        instanceId, nextAfterStart.StepName);
                }
            }
            else
            {
                await _notificationService.NotifyStepAsync(instance, targetStep, instance.BusinessType, instance.BusinessId);
            }

            _logger.LogInformation("审批退回: InstanceId={InstanceId}, FromStep={FromStep}, ToStep={ToStep}, Behavior={Behavior}",
                instanceId, currentStep.StepName, targetStep.StepName, currentStep.RejectBehavior);
        }
        else
        {
            instance.Status = 2;
            instance.CurrentStepId = null;
            instance.CompletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _notificationService.NotifyCompleteAsync(instance, instance.BusinessType, instance.BusinessId, false);

            _logger.LogInformation("审批驳回关闭: InstanceId={InstanceId}, Step={StepName}, Approver={ApproverId}, Behavior={Behavior}",
                instanceId, currentStep.StepName, approverId, currentStep.RejectBehavior);
        }

        await UpdateBusinessStatusAsync(instance, null);
    }

    public async Task TransferAsync(long instanceId, long fromUserId, long toUserId, string? comment)
    {
        var instance = await _db.ApprovalInstances
            .Include(i => i.Template)
            .ThenInclude(t => t!.Steps)
            .Include(i => i.Tasks)
            .FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance == null || instance.Status != 0)
        {
            throw new InvalidOperationException("审批实例不存在或已结束");
        }

        var currentStep = instance.Template?.Steps
            .FirstOrDefault(s => s.Id == instance.CurrentStepId);

        if (currentStep == null)
        {
            throw new InvalidOperationException("当前步骤不存在");
        }

        if (!currentStep.CanTransfer)
        {
            throw new InvalidOperationException("当前步骤不允许转交");
        }

        if (!await CanUserApproveAsync(instanceId, fromUserId))
        {
            throw new UnauthorizedAccessException("您没有权限转交此步骤");
        }

        var fromUser = await _db.Users.FindAsync(fromUserId);
        var toUser = await _db.Users.FindAsync(toUserId);

        var fromTask = instance.Tasks
            .FirstOrDefault(t => t.StepId == currentStep.Id && t.Status == 0 && t.AssigneeId == fromUserId);

        if (fromTask != null)
        {
            fromTask.Status = 2;
            fromTask.Action = "TRANSFER";
            fromTask.Comment = comment;
            fromTask.CompletedAt = DateTime.UtcNow;
        }

        var dueDate = currentStep.TimeoutHours.HasValue
            ? DateTime.UtcNow.AddHours(currentStep.TimeoutHours.Value)
            : (DateTime?)null;

        _db.ApprovalInstanceTasks.Add(new SysApprovalInstanceTask
        {
            InstanceId = instanceId,
            StepId = currentStep.Id,
            AssigneeId = toUserId,
            Status = 0,
            DueDate = dueDate,
            CreatedAt = DateTime.UtcNow
        });

        _db.ApprovalRecords.Add(new SysApprovalRecord
        {
            InstanceId = instanceId,
            StepId = currentStep.Id,
            StepName = currentStep.StepName,
            ApproverId = fromUserId,
            ApproverName = fromUser?.RealName ?? fromUser?.Username,
            Action = "TRANSFER",
            Comment = $"转交给:{toUser?.RealName ?? toUser?.Username}: {comment}",
            Site = instance.Site,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        await _notificationService.NotifyStepAsync(instance, currentStep, instance.BusinessType, instance.BusinessId);

        _logger.LogInformation("审批已转交: InstanceId={InstanceId}, From={FromUserId}, To={ToUserId}",
            instanceId, fromUserId, toUserId);
    }

    private async Task<bool> CanUserApproveAsync(long instanceId, long userId)
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
                .Where(r => userRoleIds.Contains(r.Id))
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
                .Where(r => userRoleIds.Contains(r.Id))
                .Select(r => r.RoleName)
                .ToListAsync();

            return roleNames.Contains(currentStep.ApproverRole);
        }

        return false;
    }

    private Task<SysApprovalStep?> ResolveNextStepAsync(SysApprovalInstance instance, SysApprovalStep currentStep, Dictionary<string, string> variables)
    {
        var steps = instance.Template?.Steps
            .Where(s => s.IsActive == true)
            .OrderBy(s => s.StepOrder)
            .ToList() ?? new List<SysApprovalStep>();

        if (currentStep.Conditions.Any(c => c.IsActive == true))
        {
            foreach (var condition in currentStep.Conditions.Where(c => c.IsActive == true).OrderBy(c => c.Priority))
            {
                if (EvaluateCondition(condition.Expression, variables))
                {
                    var targetStep = steps.FirstOrDefault(s => s.Id == condition.TargetStepId);
                    if (targetStep != null)
                    {
                        _logger.LogInformation("条件分支命中: InstanceId={InstanceId}, Condition={Condition}, TargetStep={TargetStep}",
                            instance.Id, condition.ConditionName, targetStep.StepName);
                        return Task.FromResult<SysApprovalStep?>(targetStep);
                    }
                }
            }
        }

        var result = steps
            .Where(s => s.StepOrder > currentStep.StepOrder)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();

        return Task.FromResult<SysApprovalStep?>(result);
    }

    private static bool EvaluateCondition(string? expression, Dictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(expression)) return false;

        var operators = new[] { ">=", "<=", "!=", "==", ">", "<" };
        string? foundOp = null;
        int opIndex = -1;

        foreach (var op in operators)
        {
            opIndex = expression.IndexOf(op, StringComparison.Ordinal);
            if (opIndex > 0)
            {
                foundOp = op;
                break;
            }
        }

        if (foundOp == null || opIndex <= 0) return false;

        var fieldName = expression[..opIndex].Trim();
        var valueStr = expression[(opIndex + foundOp.Length)..].Trim();

        if (!variables.TryGetValue(fieldName, out var fieldValue))
            return false;

        if (decimal.TryParse(fieldValue, out var fieldNum) && decimal.TryParse(valueStr, out var valueNum))
        {
            return foundOp switch
            {
                ">" => fieldNum > valueNum,
                "<" => fieldNum < valueNum,
                ">=" => fieldNum >= valueNum,
                "<=" => fieldNum <= valueNum,
                "==" => fieldNum == valueNum,
                "!=" => fieldNum != valueNum,
                _ => false
            };
        }

        var compareValue = valueStr.Trim('\'');
        return foundOp switch
        {
            "==" => string.Equals(fieldValue, compareValue, StringComparison.OrdinalIgnoreCase),
            "!=" => !string.Equals(fieldValue, compareValue, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private async Task UpdateBusinessStatusAsync(SysApprovalInstance instance, decimal? reviewCost)
    {
        var updater = _statusUpdaters.FirstOrDefault(u => u.Supports(instance.BusinessType));
        if (updater != null)
        {
            await updater.UpdateStatusAsync(instance.BusinessId, instance.Status, instance.CurrentStepId, reviewCost);
        }
    }

    private async Task<Dictionary<string, string>> GetBusinessVariablesAsync(string businessType, long businessId)
    {
        var provider = _variableProviders.FirstOrDefault(p => p.Supports(businessType));
        if (provider != null)
        {
            return await provider.GetVariablesAsync(businessId);
        }
        return new Dictionary<string, string>();
    }
}
