using CpmServer.Common;
using CpmServer.Constants;
using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using CpmServer.Modules.Approval.DTOs;
using CpmServer.Modules.Approval.Services.Approvers;
using CpmServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CpmServer.Modules.Approval.Services;

public class ApprovalInstanceService : IApprovalInstanceService
{
    private readonly CpmDbContext _db;
    private readonly ILogger<ApprovalInstanceService> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly IEmailService _emailService;
    private readonly Dictionary<string, IApproverResolver> _resolvers;
    private readonly IEnumerable<IBusinessVariableProvider> _variableProviders;
    private readonly IApprovalNotificationService _notificationService;

    public ApprovalInstanceService(
        CpmDbContext db,
        ILogger<ApprovalInstanceService> logger,
        ICurrentUser currentUser,
        IEmailService emailService,
        IEnumerable<IApproverResolver> resolvers,
        IEnumerable<IBusinessVariableProvider> variableProviders,
        IApprovalNotificationService notificationService)
    {
        _db = db;
        _logger = logger;
        _currentUser = currentUser;
        _emailService = emailService;
        _resolvers = resolvers.ToDictionary(r => r.RuleType, StringComparer.OrdinalIgnoreCase);
        _variableProviders = variableProviders;
        _notificationService = notificationService;
    }

    public async Task<SysApprovalInstance> StartApprovalAsync(string businessType, long businessId, string templateCode)
    {
        var currentApp = _currentUser.App ?? "cpm";
        var currentSite = _currentUser.Site;
        var template = await _db.ApprovalTemplates
            .Include(t => t.Steps)
            .ThenInclude(s => s.Rules)
            .FirstOrDefaultAsync(t => t.TemplateCode == templateCode && t.IsActive == true &&
                (t.App == currentApp || string.IsNullOrEmpty(t.App)) &&
                (t.Site == currentSite || string.IsNullOrEmpty(t.Site)));

        if (template == null)
        {
            throw new InvalidOperationException($"审批模板不存在: {templateCode}");
        }

        return await StartWithTemplateAsync(businessType, businessId, template, _currentUser.UserId ?? 0);
    }

    public async Task<SysApprovalInstance> StartApprovalAsync(string businessType, long businessId, string moduleType, string? site, long submitterId)
    {
        var template = await FindBestTemplateAsync(moduleType, site);
        if (template == null)
        {
            throw new InvalidOperationException($"未找到匹配的审批模板: ModuleType={moduleType}, Site={site}");
        }

        return await StartWithTemplateAsync(businessType, businessId, template, submitterId);
    }

    public async Task<SysApprovalInstance?> GetInstanceAsync(string businessType, long businessId)
    {
        var currentApp = _currentUser.App ?? "cpm";
        var currentSite = _currentUser.Site;
        return await _db.ApprovalInstances
            .Include(i => i.Template)
            .Include(i => i.Records)
            .Include(i => i.Tasks)
            .FirstOrDefaultAsync(i =>
                i.BusinessType == businessType &&
                i.BusinessId == businessId &&
                (i.App == currentApp || string.IsNullOrEmpty(i.App)) &&
                (i.Site == currentSite || string.IsNullOrEmpty(i.Site)));
    }

    public async Task<List<ApprovalForecastStepDto>> ForecastApprovalAsync(string moduleType, string businessType, long businessId, long submitterId)
    {
        var variables = await GetBusinessVariablesAsync(businessType, businessId);
        var site = await GetBusinessSiteAsync(businessType, businessId);

        var template = await FindBestTemplateAsync(moduleType, site);
        if (template == null)
        {
            _logger.LogWarning("审批预测失败: 未找到匹配的模板 ModuleType={ModuleType}, Site={Site}", moduleType, site);
            return new List<ApprovalForecastStepDto>();
        }

        var steps = await _db.ApprovalSteps
            .Include(s => s.Rules)
            .Where(s => s.TemplateId == template.Id && s.IsActive == true)
            .OrderBy(s => s.StepOrder)
            .ToListAsync();

        var result = new List<ApprovalForecastStepDto>();

        foreach (var step in steps)
        {
            if (step.StepMode == ApprovalConstants.StepMode.Start || step.StepMode == ApprovalConstants.StepMode.End || step.StepMode == ApprovalConstants.StepMode.Cc)
                continue;

            var approverResults = await ResolveApproversAsync(step, submitterId, variables);

            var approverDtos = new List<ApprovalForecastApproverDto>();
            foreach (var ar in approverResults)
            {
                if (ar.UserId.HasValue)
                {
                    var user = await _db.Users.FindAsync(ar.UserId.Value);
                    approverDtos.Add(new ApprovalForecastApproverDto
                    {
                        Type = "USER",
                        Name = user?.RealName ?? user?.Username ?? $"用户#{ar.UserId.Value}",
                        UserId = ar.UserId.Value,
                        AvatarUrl = user?.AvatarUrl
                    });
                }
                else if (!string.IsNullOrEmpty(ar.RoleCode))
                {
                    var role = await _db.Roles
                        .FirstOrDefaultAsync(r => r.RoleCode == ar.RoleCode || r.RoleName == ar.RoleCode);
                    approverDtos.Add(new ApprovalForecastApproverDto
                    {
                        Type = "ROLE",
                        Name = role?.RoleName ?? ar.RoleCode,
                        RoleCode = ar.RoleCode
                    });
                }
            }

            result.Add(new ApprovalForecastStepDto
            {
                StepId = step.Id,
                StepName = step.StepName,
                StepType = step.StepType,
                StepMode = step.StepMode,
                StepOrder = step.StepOrder,
                CanReject = step.CanReject,
                CanTransfer = step.CanTransfer,
                TimeoutHours = step.TimeoutHours,
                Approvers = approverDtos
            });
        }

        _logger.LogInformation("审批预测完成: ModuleType={ModuleType}, BusinessType={BusinessType}, BusinessId={BusinessId}, Steps={StepCount}",
            moduleType, businessType, businessId, result.Count);

        return result;
    }

    public async Task ScanTimeoutTasksAsync()
    {
        var now = DateTime.UtcNow;
        var currentApp = _currentUser.App ?? "cpm";
        var currentSite = _currentUser.Site;
        var timeoutTasks = await _db.ApprovalInstanceTasks
            .Include(t => t.Instance)
            .ThenInclude(i => i!.Template)
            .Include(t => t.Assignee)
            .Where(t => t.Status == ApprovalConstants.TaskStatus.Pending && t.DueDate != null && t.DueDate < now &&
                (t.App == currentApp || string.IsNullOrEmpty(t.App)) &&
                (t.Site == currentSite || string.IsNullOrEmpty(t.Site)))
            .ToListAsync();

        foreach (var task in timeoutTasks)
        {
            try
            {
                if (task.AssigneeId.HasValue && !string.IsNullOrEmpty(task.Assignee?.Email))
                {
                    var variables = task.Instance != null
                        ? await GetBusinessVariablesAsync(task.Instance.BusinessType, task.Instance.BusinessId)
                        : new Dictionary<string, string>();

                    variables["StepName"] = task.Instance?.Template?.Steps
                        .FirstOrDefault(s => s.Id == task.StepId)?.StepName ?? "未知步骤";
                    variables["DueDate"] = task.DueDate?.ToString("yyyy-MM-dd HH:mm") ?? "";

                    await _emailService.SendEmailByTemplateAsync(EmailTemplateConstants.ApprovalTimeout, variables, task.Assignee.Email);
                }

                task.Status = 3;
                _logger.LogInformation("任务已超时: TaskId={TaskId}, InstanceId={InstanceId}, StepId={StepId}",
                    task.Id, task.InstanceId, task.StepId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "超时任务处理失败: TaskId={TaskId}", task.Id);
            }
        }

        if (timeoutTasks.Any())
        {
            await _db.SaveChangesAsync();
        }
    }

    private async Task<SysApprovalTemplate?> FindBestTemplateAsync(string moduleType, string? site)
    {
        var currentApp = _currentUser.App ?? "cpm";
        var templates = await _db.ApprovalTemplates
            .Include(t => t.Steps)
            .ThenInclude(s => s.Rules)
            .Where(t => t.ModuleType == moduleType && t.IsActive == true &&
                (t.App == currentApp || string.IsNullOrEmpty(t.App)))
            .OrderBy(t => t.Site == null ? 1 : 0)
            .ThenByDescending(t => t.IsDefault)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

        if (!string.IsNullOrEmpty(site))
        {
            var exactMatch = templates.FirstOrDefault(t => t.Site == site);
            if (exactMatch != null) return exactMatch;
        }

        var globalMatch = templates.FirstOrDefault(t => t.Site == null);
        return globalMatch;
    }

    private async Task<SysApprovalInstance> StartWithTemplateAsync(string businessType, long businessId, SysApprovalTemplate template, long submitterId)
    {
        var steps = template.Steps
            .Where(s => s.IsActive == true)
            .OrderBy(s => s.StepOrder)
            .ToList();

        if (!steps.Any())
        {
            throw new InvalidOperationException("审批模板没有配置步骤");
        }

        var firstStep = steps.First();

        string? businessSite = null;
        var siteProvider = _variableProviders.FirstOrDefault(p => p.Supports(businessType));
        if (siteProvider != null)
        {
            businessSite = await siteProvider.GetBusinessSiteAsync(businessId);
        }

        var variables = await GetBusinessVariablesAsync(businessType, businessId);

        var currentApp = _currentUser.App ?? "cpm";
        var instance = new SysApprovalInstance
        {
            TemplateId = template.Id,
            BusinessType = businessType,
            BusinessId = businessId,
            CurrentStepId = firstStep.Id,
            CurrentStepOrder = firstStep.StepOrder,
            Site = businessSite ?? template.Site ?? _currentUser.Site,
            App = currentApp,
            Status = 0,
            SubmitterId = submitterId,
            Variables = System.Text.Json.JsonSerializer.Serialize(variables),
            CreatedAt = DateTime.UtcNow
        };

        _db.ApprovalInstances.Add(instance);
        await _db.SaveChangesAsync();

        if (firstStep.StepMode == ApprovalConstants.StepMode.Start)
        {
            _logger.LogInformation("流程 START 节点启动，自动跳过: InstanceId={InstanceId}", instance.Id);
            var secondStep = steps.Skip(1).FirstOrDefault();
            if (secondStep != null)
            {
                instance.CurrentStepId = secondStep.Id;
                instance.CurrentStepOrder = secondStep.StepOrder;
                await _db.SaveChangesAsync();
                await CreateStepTasksAsync(instance, secondStep, submitterId, variables);
                await _notificationService.NotifyStepAsync(instance, secondStep, businessType, businessId);
            }
        }
        else
        {
            await CreateStepTasksAsync(instance, firstStep, submitterId, variables);
            await _notificationService.NotifyStepAsync(instance, firstStep, businessType, businessId);
        }

        _logger.LogInformation("审批流程已启动: BusinessType={BusinessType}, BusinessId={BusinessId}, Template={TemplateCode}, Submitter={SubmitterId}",
            businessType, businessId, template.TemplateCode, submitterId);

        return instance;
    }

    internal async Task CreateStepTasksAsync(SysApprovalInstance instance, SysApprovalStep step, long submitterId, Dictionary<string, string> variables)
    {
        if (step.StepMode == ApprovalConstants.StepMode.Start)
        {
            _logger.LogInformation("开始节点跳过: InstanceId={InstanceId}, Step={StepName}", instance.Id, step.StepName);
            return;
        }

        if (step.StepMode == ApprovalConstants.StepMode.End)
        {
            _logger.LogInformation("结束节点: InstanceId={InstanceId}, Step={StepName}", instance.Id, step.StepName);
            instance.Status = 1;
            instance.CurrentStepId = null;
            instance.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await _notificationService.NotifyCompleteAsync(instance, instance.BusinessType, instance.BusinessId, true);
            return;
        }

        if (step.StepMode == ApprovalConstants.StepMode.Cc)
        {
            _logger.LogInformation("抄送节点: InstanceId={InstanceId}, Step={StepName}", instance.Id, step.StepName);
            return;
        }

        var approvers = await ResolveApproversAsync(step, submitterId, variables);

        if (!approvers.Any())
        {
            _logger.LogWarning("步骤 {StepName} 未解析到任何审批人", step.StepName);
            return;
        }

        var dueDate = step.TimeoutHours.HasValue
            ? DateTime.UtcNow.AddHours(step.TimeoutHours.Value)
            : (DateTime?)null;

        foreach (var approver in approvers)
        {
            var task = new SysApprovalInstanceTask
            {
                InstanceId = instance.Id,
                StepId = step.Id,
                AssigneeId = approver.UserId,
                AssigneeRole = approver.RoleCode,
                Status = 0,
                DueDate = dueDate,
                Site = instance.Site,
                App = instance.App,
                CreatedAt = DateTime.UtcNow
            };
            _db.ApprovalInstanceTasks.Add(task);
        }

        await _db.SaveChangesAsync();
    }

    internal async Task<List<ApproverResult>> ResolveApproversAsync(SysApprovalStep step, long submitterId, Dictionary<string, string> variables)
    {
        var results = new List<ApproverResult>();

        if (step.Rules.Any(r => r.IsActive == true))
        {
            var context = new ApproverContext
            {
                SubmitterId = submitterId,
                SubmitterSite = variables.GetValueOrDefault("Site"),
                InstanceId = null,
                Amount = decimal.TryParse(variables.GetValueOrDefault("TotalAmount"), out var amt) ? amt : null
            };

            foreach (var rule in step.Rules.Where(r => r.IsActive == true).OrderBy(r => r.Priority))
            {
                if (_resolvers.TryGetValue(rule.RuleType, out var resolver))
                {
                    var result = await resolver.ResolveAsync(rule, context, _db);
                    if (result.Success)
                    {
                        results.Add(result);
                    }
                }
            }
        }

        if (!results.Any())
        {
            if (step.ApproverUserId.HasValue)
            {
                results.Add(new ApproverResult { UserId = step.ApproverUserId.Value });
            }
            else if (!string.IsNullOrEmpty(step.ApproverRole))
            {
                results.Add(new ApproverResult { RoleCode = step.ApproverRole });
            }
        }

        return results;
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

    private async Task<string?> GetBusinessSiteAsync(string businessType, long businessId)
    {
        var provider = _variableProviders.FirstOrDefault(p => p.Supports(businessType));
        if (provider != null)
        {
            return await provider.GetBusinessSiteAsync(businessId);
        }
        return null;
    }
}
