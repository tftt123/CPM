using CpmServer.Common;
using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using CpmServer.Modules.Approval.DTOs;
using CpmServer.Modules.Approval.Services.Approvers;
using CpmServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CpmServer.Modules.Approval.Services;

public class ApprovalService : IApprovalService
{
    private readonly CpmDbContext _db;
    private readonly ILogger<ApprovalService> _logger;
    private readonly IEmailService _emailService;
    private readonly ICurrentUser _currentUser;
    private readonly Dictionary<string, IApproverResolver> _resolvers;
    private readonly IEnumerable<IBusinessVariableProvider> _variableProviders;
    private readonly IEnumerable<IBusinessStatusUpdater> _statusUpdaters;

    public ApprovalService(
        CpmDbContext db,
        ILogger<ApprovalService> logger,
        IEmailService emailService,
        ICurrentUser currentUser,
        IEnumerable<IApproverResolver> resolvers,
        IEnumerable<IBusinessVariableProvider> variableProviders,
        IEnumerable<IBusinessStatusUpdater> statusUpdaters)
    {
        _db = db;
        _logger = logger;
        _emailService = emailService;
        _currentUser = currentUser;
        _resolvers = resolvers.ToDictionary(r => r.RuleType, StringComparer.OrdinalIgnoreCase);
        _variableProviders = variableProviders;
        _statusUpdaters = statusUpdaters;
    }

    #region 审批模板管理

    public async Task<List<ApprovalTemplateDto>> GetTemplatesAsync(string? moduleType)
    {
        var query = _db.ApprovalTemplates
            .Include(t => t.Steps)
            .ThenInclude(s => s.Rules)
            .Include(t => t.Steps)
            .ThenInclude(s => s.Conditions)
            .AsQueryable();

        if (!string.IsNullOrEmpty(moduleType))
        {
            query = query.Where(t => t.ModuleType == moduleType);
        }

        var list = await query.OrderBy(t => t.TemplateCode).ToListAsync();

        return list.Select(t => MapToDto(t)).ToList();
    }

    public async Task<ApprovalTemplateDto?> GetTemplateByIdAsync(long id)
    {
        var entity = await _db.ApprovalTemplates
            .Include(t => t.Steps)
            .ThenInclude(s => s.Rules)
            .Include(t => t.Steps)
            .ThenInclude(s => s.Conditions)
            .FirstOrDefaultAsync(t => t.Id == id);

        return entity == null ? null : MapToDto(entity);
    }

    private string GenerateTemplateCode(string moduleType)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var suffix = Random.Shared.Next(1000, 9999).ToString("X");
        return $"{moduleType}-{timestamp}-{suffix}";
    }

    public async Task<long> CreateTemplateAsync(ApprovalTemplateDto dto)
    {
        var entity = new SysApprovalTemplate
        {
            TemplateCode = GenerateTemplateCode(dto.ModuleType),
            TemplateName = dto.TemplateName,
            ModuleType = dto.ModuleType,
            Description = dto.Description,
            IsDefault = dto.IsDefault,
            IsActive = dto.IsActive,
            Site = !string.IsNullOrWhiteSpace(dto.Site) ? dto.Site : _currentUser.Site,
            CreatedAt = DateTime.UtcNow,
            Steps = new List<SysApprovalStep>()
        };

        _db.ApprovalTemplates.Add(entity);

        foreach (var step in dto.Steps.OrderBy(s => s.StepOrder))
        {
            var stepEntity = new SysApprovalStep
            {
                Template = entity,
                StepName = step.StepName,
                StepOrder = step.StepOrder,
                StepType = step.StepType,
                StepMode = step.StepMode,
                ApproverRole = step.ApproverRole,
                ApproverUserId = step.ApproverUserId,
                CanReject = step.CanReject,
                CanTransfer = step.CanTransfer,
                NotifyEmailTemplate = step.NotifyEmailTemplate,
                RejectBehavior = step.RejectBehavior,
                RejectTargetStepId = step.RejectTargetStepId,
                TimeoutHours = step.TimeoutHours,
                Rules = new List<SysApprovalRule>(),
                Conditions = new List<SysApprovalCondition>()
            };
            entity.Steps.Add(stepEntity);

            foreach (var rule in step.Rules)
            {
                stepEntity.Rules.Add(new SysApprovalRule
                {
                    RuleType = rule.RuleType,
                    RuleValue = rule.RuleValue,
                    Fallback = rule.Fallback,
                    Priority = rule.Priority,
                    IsActive = rule.IsActive
                });
            }

            foreach (var cond in step.Conditions)
            {
                stepEntity.Conditions.Add(new SysApprovalCondition
                {
                    ConditionName = cond.ConditionName,
                    Expression = cond.Expression,
                    TargetStepId = cond.TargetStepId,
                    Priority = cond.Priority,
                    IsActive = cond.IsActive
                });
            }
        }

        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateTemplateAsync(long id, ApprovalTemplateDto dto)
    {
        var entity = await _db.ApprovalTemplates
            .Include(t => t.Steps)
            .ThenInclude(s => s.Rules)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (entity == null) return;

        entity.TemplateName = dto.TemplateName;
        entity.ModuleType = dto.ModuleType;
        entity.Description = dto.Description;
        entity.IsDefault = dto.IsDefault;
        entity.IsActive = dto.IsActive;
        entity.Site = !string.IsNullOrWhiteSpace(dto.Site) ? dto.Site : entity.Site;

        // 软删除旧步骤，同时清理关联的 Rules Conditions
        foreach (var step in entity.Steps.Where(s => s.IsActive == true).ToList())
        {
            step.IsActive = false;
            foreach (var rule in step.Rules.ToList())
            {
                _db.ApprovalRules.Remove(rule);
            }
            foreach (var cond in step.Conditions.ToList())
            {
                _db.ApprovalConditions.Remove(cond);
            }
        }

        foreach (var stepDto in dto.Steps.OrderBy(s => s.StepOrder))
        {
            if (stepDto.Id.HasValue && entity.Steps.Any(s => s.Id == stepDto.Id.Value))
            {
                var existing = entity.Steps.First(s => s.Id == stepDto.Id.Value);
                existing.StepName = stepDto.StepName;
                existing.StepOrder = stepDto.StepOrder;
                existing.StepType = stepDto.StepType;
                existing.StepMode = stepDto.StepMode;
                existing.ApproverRole = stepDto.ApproverRole;
                existing.ApproverUserId = stepDto.ApproverUserId;
                existing.CanReject = stepDto.CanReject;
                existing.CanTransfer = stepDto.CanTransfer;
                existing.NotifyEmailTemplate = stepDto.NotifyEmailTemplate;
                existing.RejectBehavior = stepDto.RejectBehavior;
                existing.RejectTargetStepId = stepDto.RejectTargetStepId;
                existing.TimeoutHours = stepDto.TimeoutHours;
                existing.IsActive = true;

                // 更新规则
                var existingRuleIds = existing.Rules.Select(r => r.Id).ToHashSet();
                var dtoRuleIds = stepDto.Rules.Where(r => r.Id.HasValue).Select(r => r.Id!.Value).ToHashSet();

                foreach (var oldRule in existing.Rules.Where(r => !dtoRuleIds.Contains(r.Id)).ToList())
                {
                    _db.ApprovalRules.Remove(oldRule);
                }

                foreach (var ruleDto in stepDto.Rules)
                {
                    if (ruleDto.Id.HasValue && existing.Rules.Any(r => r.Id == ruleDto.Id.Value))
                    {
                        var existingRule = existing.Rules.First(r => r.Id == ruleDto.Id.Value);
                        existingRule.RuleType = ruleDto.RuleType;
                        existingRule.RuleValue = ruleDto.RuleValue;
                        existingRule.Fallback = ruleDto.Fallback;
                        existingRule.Priority = ruleDto.Priority;
                        existingRule.IsActive = ruleDto.IsActive;
                    }
                    else
                    {
                        _db.ApprovalRules.Add(new SysApprovalRule
                        {
                            StepId = existing.Id,
                            RuleType = ruleDto.RuleType,
                            RuleValue = ruleDto.RuleValue,
                            Fallback = ruleDto.Fallback,
                            Priority = ruleDto.Priority,
                            IsActive = ruleDto.IsActive
                        });
                    }
                }

                // 更新条件分支
                var existingCondIds = existing.Conditions.Select(c => c.Id).ToHashSet();
                var dtoCondIds = stepDto.Conditions.Where(c => c.Id.HasValue).Select(c => c.Id!.Value).ToHashSet();

                foreach (var oldCond in existing.Conditions.Where(c => !dtoCondIds.Contains(c.Id)).ToList())
                {
                    _db.ApprovalConditions.Remove(oldCond);
                }

                foreach (var condDto in stepDto.Conditions)
                {
                    if (condDto.Id.HasValue && existing.Conditions.Any(c => c.Id == condDto.Id.Value))
                    {
                        var existingCond = existing.Conditions.First(c => c.Id == condDto.Id.Value);
                        existingCond.ConditionName = condDto.ConditionName;
                        existingCond.Expression = condDto.Expression;
                        existingCond.TargetStepId = condDto.TargetStepId;
                        existingCond.Priority = condDto.Priority;
                        existingCond.IsActive = condDto.IsActive;
                    }
                    else
                    {
                        _db.ApprovalConditions.Add(new SysApprovalCondition
                        {
                            StepId = existing.Id,
                            ConditionName = condDto.ConditionName,
                            Expression = condDto.Expression,
                            TargetStepId = condDto.TargetStepId,
                            Priority = condDto.Priority,
                            IsActive = condDto.IsActive
                        });
                    }
                }
            }
            else
            {
                var newStep = new SysApprovalStep
                {
                    TemplateId = entity.Id,
                    StepName = stepDto.StepName,
                    StepOrder = stepDto.StepOrder,
                    StepType = stepDto.StepType,
                    StepMode = stepDto.StepMode,
                    ApproverRole = stepDto.ApproverRole,
                    ApproverUserId = stepDto.ApproverUserId,
                    CanReject = stepDto.CanReject,
                    CanTransfer = stepDto.CanTransfer,
                    NotifyEmailTemplate = stepDto.NotifyEmailTemplate,
                    RejectBehavior = stepDto.RejectBehavior,
                    RejectTargetStepId = stepDto.RejectTargetStepId,
                    TimeoutHours = stepDto.TimeoutHours
                };
                _db.ApprovalSteps.Add(newStep);
                await _db.SaveChangesAsync();

                foreach (var ruleDto in stepDto.Rules)
                {
                    _db.ApprovalRules.Add(new SysApprovalRule
                    {
                        StepId = newStep.Id,
                        RuleType = ruleDto.RuleType,
                        RuleValue = ruleDto.RuleValue,
                        Fallback = ruleDto.Fallback,
                        Priority = ruleDto.Priority,
                        IsActive = ruleDto.IsActive
                    });
                }

                foreach (var condDto in stepDto.Conditions)
                {
                    _db.ApprovalConditions.Add(new SysApprovalCondition
                    {
                        StepId = newStep.Id,
                        ConditionName = condDto.ConditionName,
                        Expression = condDto.Expression,
                        TargetStepId = condDto.TargetStepId,
                        Priority = condDto.Priority,
                        IsActive = condDto.IsActive
                    });
                }
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteTemplateAsync(long id)
    {
        var entity = await _db.ApprovalTemplates.FindAsync(id);
        if (entity != null)
        {
            entity.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }

    #endregion

    #region 审批流程执行

    /// <summary>
    /// 旧方法保留：按模板编码启动（兼容现有调用）
    /// </summary>
    public async Task<SysApprovalInstance> StartApprovalAsync(string businessType, long businessId, string templateCode)
    {
        var template = await _db.ApprovalTemplates
            .Include(t => t.Steps)
            .ThenInclude(s => s.Rules)
            .FirstOrDefaultAsync(t => t.TemplateCode == templateCode && t.IsActive == true);

        if (template == null)
        {
            throw new InvalidOperationException($"审批模板不存在: {templateCode}");
        }

        return await StartWithTemplateAsync(businessType, businessId, template, _currentUser.UserId ?? 0);
    }

    /// <summary>
    /// Phase 1 新增：按 ModuleType + Site 自动匹配模板启动
    /// </summary>
    public async Task<SysApprovalInstance> StartApprovalAsync(string businessType, long businessId, string moduleType, string? site, long submitterId)
    {
        var template = await FindBestTemplateAsync(moduleType, site);
        if (template == null)
        {
            throw new InvalidOperationException($"未找到匹配的审批模板: ModuleType={moduleType}, Site={site}");
        }

        return await StartWithTemplateAsync(businessType, businessId, template, submitterId);
    }

    /// <summary>
    /// 模板匹配引擎：优先 Site 精确匹配，其次全局模板
    /// </summary>
    private async Task<SysApprovalTemplate?> FindBestTemplateAsync(string moduleType, string? site)
    {
        var templates = await _db.ApprovalTemplates
            .Include(t => t.Steps)
            .ThenInclude(s => s.Rules)
            .Where(t => t.ModuleType == moduleType && t.IsActive == true)
            .OrderBy(t => t.Site == null ? 1 : 0)  // Site 非空优先
            .ThenByDescending(t => t.IsDefault)     // 默认模板优先
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

        // 1. 精确匹配 Site
        if (!string.IsNullOrEmpty(site))
        {
            var exactMatch = templates.FirstOrDefault(t => t.Site == site);
            if (exactMatch != null) return exactMatch;
        }

        // 2. 使用全局模板（Site 为 null）
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

        // 从业务对象获取 Site
        string? businessSite = null;
        var siteProvider = _variableProviders.FirstOrDefault(p => p.Supports(businessType));
        if (siteProvider != null)
        {
            businessSite = await siteProvider.GetBusinessSiteAsync(businessId);
        }

        // 构建流程变量
        var variables = await GetBusinessVariablesAsync(businessType, businessId);

        var instance = new SysApprovalInstance
        {
            TemplateId = template.Id,
            BusinessType = businessType,
            BusinessId = businessId,
            CurrentStepId = firstStep.Id,
            CurrentStepOrder = firstStep.StepOrder,
            Site = businessSite ?? template.Site ?? _currentUser.Site,
            Status = 0,
            SubmitterId = submitterId,
            Variables = System.Text.Json.JsonSerializer.Serialize(variables),
            CreatedAt = DateTime.UtcNow
        };

        _db.ApprovalInstances.Add(instance);
        await _db.SaveChangesAsync();

        // 如果第一步是 START 节点，自动跳过并创建下一步的待办
        if (firstStep.StepMode == "START")
        {
            _logger.LogInformation("流程 START 节点启动，自动跳过: InstanceId={InstanceId}", instance.Id);
            var secondStep = steps.Skip(1).FirstOrDefault();
            if (secondStep != null)
            {
                instance.CurrentStepId = secondStep.Id;
                instance.CurrentStepOrder = secondStep.StepOrder;
                await _db.SaveChangesAsync();
                await CreateStepTasksAsync(instance, secondStep, submitterId, variables);
                await NotifyStepAsync(instance, secondStep, businessType, businessId);
            }
        }
        else
        {
            // 创建第一步的待办任务
            await CreateStepTasksAsync(instance, firstStep, submitterId, variables);
            // 发送第一步通知
            await NotifyStepAsync(instance, firstStep, businessType, businessId);
        }

        _logger.LogInformation("审批流程已启动: BusinessType={BusinessType}, BusinessId={BusinessId}, Template={TemplateCode}, Submitter={SubmitterId}",
            businessType, businessId, template.TemplateCode, submitterId);

        return instance;
    }

    public async Task<SysApprovalInstance?> GetInstanceAsync(string businessType, long businessId)
    {
        return await _db.ApprovalInstances
            .Include(i => i.Template)
            .Include(i => i.Records)
            .Include(i => i.Tasks)
            .FirstOrDefaultAsync(i => i.BusinessType == businessType && i.BusinessId == businessId);
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

    /// <summary>
    /// 获取用户的待办任务列表
    /// </summary>
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
            .Where(r => userRoleIds.Contains(r.Id))
            .Select(r => r.RoleName)
            .ToListAsync();

        // 查询指派给该用户或该用户角色的待办任务
        var tasks = await _db.ApprovalInstanceTasks
            .Include(t => t.Instance)
            .ThenInclude(i => i!.Template)
            .ThenInclude(t => t!.Steps)
            .Include(t => t.Assignee)
            .Where(t => t.Status == 0 &&
                (t.AssigneeId == userId || (t.AssigneeRole != null && userRoleNames.Contains(t.AssigneeRole))))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(t => MapTaskToDto(t)).ToList();
    }

    /// <summary>
    /// 获取某个审批实例的所有任务
    /// </summary>
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

    #endregion

    #region 审批操作

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

        // 权限检查（兼容旧字段 + Task 体系）
        if (!await CanUserApproveAsync(instanceId, approverId))
        {
            throw new UnauthorizedAccessException("您没有权限审批此步骤");
        }

        var approver = await _db.Users.FindAsync(approverId);

        // 记录审批
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

        // 更新当前用户的待办任务为已处理
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

        // 检查是否还有其他未处理的待办（会签场景）
        var pendingTasks = await _db.ApprovalInstanceTasks
            .Where(t => t.InstanceId == instanceId && t.StepId == currentStep.Id && t.Status == 0)
            .ToListAsync();

        // 会签模式判断
        bool shouldAdvance = currentStep.StepMode switch
        {
            "PARALLEL" => !pendingTasks.Any(t => t.Status == 0 && t.Id != myTask?.Id), // 全部通过
            "PARALLEL_ANY" => true, // 任一通过即继续
            "CC" => true, // 抄送节点无需审批，直接推进
            _ => true
        };

        // PARALLEL_ANY：将其他未处理的待办标记为跳过
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

            // 查找下一步（支持条件分支）
            var nextStep = await ResolveNextStepAsync(instance, currentStep, variables);

            if (nextStep != null)
            {
                instance.CurrentStepId = nextStep.Id;
                instance.CurrentStepOrder = nextStep.StepOrder;
                await CreateStepTasksAsync(instance, nextStep, instance.SubmitterId ?? 0, variables);

                // 如果下一步是 END 节点，自动处理结束
                if (nextStep.StepMode == "END")
                {
                    _logger.LogInformation("到达结束节点，流程自动完成: InstanceId={InstanceId}", instance.Id);
                }
            }
            else
            {
                // 流程完成（没有下一步了）
                instance.Status = 1;
                instance.CurrentStepId = null;
                instance.CompletedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();

        // 更新业务单据状态
        await UpdateBusinessStatusAsync(instance, reviewCost);

        // 发送下一步通知（如果有）
        if (instance.Status == 0 && shouldAdvance)
        {
            var variables = instance.Variables != null
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(instance.Variables) ?? new Dictionary<string, string>()
                : new Dictionary<string, string>();
            var nextStep = await ResolveNextStepAsync(instance, currentStep, variables);
            if (nextStep != null && nextStep.StepMode != "END")
            {
                await NotifyStepAsync(instance, nextStep, instance.BusinessType, instance.BusinessId);
            }
        }
        else if (instance.Status == 1)
        {
            await NotifyCompleteAsync(instance, instance.BusinessType, instance.BusinessId, true);
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

        // 权限检查
        if (!await CanUserApproveAsync(instanceId, approverId))
        {
            throw new UnauthorizedAccessException("您没有权限审批此步骤");
        }

        var approver = await _db.Users.FindAsync(approverId);

        // 记录驳回
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

        // 更新任务状态
        foreach (var task in instance.Tasks.Where(t => t.StepId == currentStep.Id && t.Status == 0))
        {
            task.Status = 1;
            task.Action = "REJECT";
            task.Comment = comment;
            task.CompletedAt = DateTime.UtcNow;
        }

        // 驳回行为处理
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
            // 退回指定步骤：流程继续
            instance.CurrentStepId = targetStep.Id;
            instance.CurrentStepOrder = targetStep.StepOrder;
            instance.Status = 0;
            instance.CompletedAt = null;

            await _db.SaveChangesAsync();

            // 创建退回步骤的待办任务
            var variables = instance.Variables != null
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(instance.Variables) ?? new Dictionary<string, string>()
                : new Dictionary<string, string>();
            await CreateStepTasksAsync(instance, targetStep, instance.SubmitterId ?? 0, variables);

            // 如果退回的START/Requestor 节点，自动推进到下一步（不需要提交人再点一次）
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
                    await CreateStepTasksAsync(instance, nextAfterStart, instance.SubmitterId ?? 0, variables);
                    if (nextAfterStart.StepMode != "END")
                    {
                        await NotifyStepAsync(instance, nextAfterStart, instance.BusinessType, instance.BusinessId);
                    }
                    _logger.LogInformation("退回发起人后自动推进: InstanceId={InstanceId}, NextStep={NextStep}",
                        instanceId, nextAfterStart.StepName);
                }
            }
            else
            {
                // 发送退回通知
                await NotifyStepAsync(instance, targetStep, instance.BusinessType, instance.BusinessId);
            }

            _logger.LogInformation("审批退回: InstanceId={InstanceId}, FromStep={FromStep}, ToStep={ToStep}, Behavior={Behavior}",
                instanceId, currentStep.StepName, targetStep.StepName, currentStep.RejectBehavior);
        }
        else
        {
            // REJECT_AND_CLOSE 或其他未匹配行为：关闭流程
            instance.Status = 2;
            instance.CurrentStepId = null;
            instance.CompletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // 发送驳回通知
            await NotifyCompleteAsync(instance, instance.BusinessType, instance.BusinessId, false);

            _logger.LogInformation("审批驳回关闭: InstanceId={InstanceId}, Step={StepName}, Approver={ApproverId}, Behavior={Behavior}",
                instanceId, currentStep.StepName, approverId, currentStep.RejectBehavior);
        }

        // 更新业务单据状态
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

        // 标记原审批人的待办为转交状态
        var fromTask = instance.Tasks
            .FirstOrDefault(t => t.StepId == currentStep.Id && t.Status == 0 && t.AssigneeId == fromUserId);

        if (fromTask != null)
        {
            fromTask.Status = 2; // 转交
            fromTask.Action = "TRANSFER";
            fromTask.Comment = comment;
            fromTask.CompletedAt = DateTime.UtcNow;
        }

        // 创建新待办任务给被转交人
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

        // 记录转交
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

        // 发送转交通知
        await NotifyStepAsync(instance, currentStep, instance.BusinessType, instance.BusinessId);

        _logger.LogInformation("审批已转交: InstanceId={InstanceId}, From={FromUserId}, To={ToUserId}",
            instanceId, fromUserId, toUserId);
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

        // 优先检查 Task 体系（Phase 1 新增）
        var pendingTasks = instance.Tasks.Where(t => t.StepId == currentStep.Id && t.Status == 0).ToList();
        if (pendingTasks.Any())
        {
            // 检查是否有指派给该用户的待办
            var directTask = pendingTasks.FirstOrDefault(t => t.AssigneeId == userId);
            if (directTask != null) return true;

            // 检查角色待办
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

        // 兼容旧字段
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

    /// <summary>
    /// 扫描超时任务：对超时的待办任务发送提醒邮件（Hangfire 定时调用）
    /// </summary>
    public async Task ScanTimeoutTasksAsync()
    {
        var now = DateTime.UtcNow;
        var timeoutTasks = await _db.ApprovalInstanceTasks
            .Include(t => t.Instance)
            .ThenInclude(i => i!.Template)
            .Include(t => t.Assignee)
            .Where(t => t.Status == 0 && t.DueDate != null && t.DueDate < now)
            .ToListAsync();

        foreach (var task in timeoutTasks)
        {
            try
            {
                // 发送超时提醒邮件
                if (task.AssigneeId.HasValue && !string.IsNullOrEmpty(task.Assignee?.Email))
                {
                    var variables = task.Instance != null
                        ? await GetBusinessVariablesAsync(task.Instance.BusinessType, task.Instance.BusinessId)
                        : new Dictionary<string, string>();

                    variables["StepName"] = task.Instance?.Template?.Steps
                        .FirstOrDefault(s => s.Id == task.StepId)?.StepName ?? "未知步骤";
                    variables["DueDate"] = task.DueDate?.ToString("yyyy-MM-dd HH:mm") ?? "";

                    await _emailService.SendEmailByTemplateAsync("APPROVAL_TIMEOUT", variables, task.Assignee.Email);
                }

                // 标记为超时状态（3=超时）
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

    #endregion

    #region 审批预测（Forecast）

    /// <summary>
    /// 审批预测：不创建实例，根据模板和业务数据预览完整审批路线
    /// </summary>
    public async Task<List<ApprovalForecastStepDto>> ForecastApprovalAsync(string moduleType, string businessType, long businessId, long submitterId)
    {
        // 1. 获取业务变量（Site、金额等用于模板匹配和审批人解析）
        var variables = await GetBusinessVariablesAsync(businessType, businessId);
        var site = await GetBusinessSiteAsync(businessType, businessId);

        // 2. 匹配最佳模板
        var template = await FindBestTemplateAsync(moduleType, site);
        if (template == null)
        {
            _logger.LogWarning("审批预测失败: 未找到匹配的模板 ModuleType={ModuleType}, Site={Site}", moduleType, site);
            return new List<ApprovalForecastStepDto>();
        }

        // 3. 加载模板步骤（含 Rules，用于审批人解析）
        var steps = await _db.ApprovalSteps
            .Include(s => s.Rules)
            .Where(s => s.TemplateId == template.Id && s.IsActive == true)
            .OrderBy(s => s.StepOrder)
            .ToListAsync();

        var result = new List<ApprovalForecastStepDto>();

        foreach (var step in steps)
        {
            // 跳过非审批节点
            if (step.StepMode == "START" || step.StepMode == "END" || step.StepMode == "CC")
                continue;

            // 4. 解析该步骤的审批人（模拟，不创建任务）
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
                    // 尝试找到角色显示名
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

    #endregion

    #region 私有方法

    /// <summary>
    /// 审批人解析引擎：按规则动态推导审批人
    /// </summary>
    private async Task<List<ApproverResult>> ResolveApproversAsync(SysApprovalStep step, long submitterId, Dictionary<string, string> variables)
    {
        var results = new List<ApproverResult>();

        // 优先使用 Rule 引擎（Phase 1）
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

        // 兼容旧字段：没有规则时使用旧字段
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

    /// <summary>
    /// 创建步骤待办任务
    /// </summary>
    private async Task CreateStepTasksAsync(SysApprovalInstance instance, SysApprovalStep step, long submitterId, Dictionary<string, string> variables)
    {
        // START 节点：不创建待办，直接推进到下一步
        if (step.StepMode == "START")
        {
            _logger.LogInformation("开始节点跳过: InstanceId={InstanceId}, Step={StepName}", instance.Id, step.StepName);
            return;
        }

        // END 节点：标记流程完成
        if (step.StepMode == "END")
        {
            _logger.LogInformation("结束节点: InstanceId={InstanceId}, Step={StepName}", instance.Id, step.StepName);
            instance.Status = 1;
            instance.CurrentStepId = null;
            instance.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await NotifyCompleteAsync(instance, instance.BusinessType, instance.BusinessId, true);
            return;
        }

        // CC 抄送节点：只通知，不创建审批任务
        if (step.StepMode == "CC")
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
                CreatedAt = DateTime.UtcNow
            };
            _db.ApprovalInstanceTasks.Add(task);
        }

        await _db.SaveChangesAsync();
    }

    private async Task NotifyStepAsync(SysApprovalInstance instance, SysApprovalStep step, string businessType, long businessId)
    {
        if (string.IsNullOrEmpty(step.NotifyEmailTemplate)) return;

        try
        {
            var variables = await GetBusinessVariablesAsync(businessType, businessId);
            variables["StepName"] = step.StepName;
            variables["ApproverName"] = await GetApproverNameAsync(step);

            var emails = await GetApproverEmailsAsync(step);
            foreach (var email in emails)
            {
                await _emailService.SendEmailByTemplateAsync(step.NotifyEmailTemplate, variables, email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "审批步骤通知发送失败: InstanceId={InstanceId}, Step={StepName}",
                instance.Id, step.StepName);
        }
    }

    private async Task NotifyCompleteAsync(SysApprovalInstance instance, string businessType, long businessId, bool approved)
    {
        try
        {
            var templateCode = approved
                ? $"{businessType.ToUpperInvariant()}_APPROVED"
                : $"{businessType.ToUpperInvariant()}_REJECTED";
            var variables = await GetBusinessVariablesAsync(businessType, businessId);

            var creatorEmail = await GetBusinessCreatorEmailAsync(businessType, businessId);
            if (!string.IsNullOrEmpty(creatorEmail))
            {
                await _emailService.SendEmailByTemplateAsync(templateCode, variables, creatorEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "审批完成通知发送失败: InstanceId={InstanceId}", instance.Id);
        }
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

    private async Task<string?> GetBusinessSiteAsync(string businessType, long businessId)
    {
        var provider = _variableProviders.FirstOrDefault(p => p.Supports(businessType));
        if (provider != null)
        {
            return await provider.GetBusinessSiteAsync(businessId);
        }
        return null;
    }

    private async Task<string?> GetBusinessCreatorEmailAsync(string businessType, long businessId)
    {
        var provider = _variableProviders.FirstOrDefault(p => p.Supports(businessType));
        if (provider != null)
        {
            return await provider.GetCreatorEmailAsync(businessId);
        }
        return null;
    }

    private async Task<string> GetApproverNameAsync(SysApprovalStep step)
    {
        if (step.ApproverUserId.HasValue)
        {
            var user = await _db.Users.FindAsync(step.ApproverUserId.Value);
            return user?.RealName ?? user?.Username ?? "未知";
        }

        if (!string.IsNullOrEmpty(step.ApproverRole))
        {
            return step.ApproverRole;
        }

        return "未知";
    }

    private async Task<List<string>> GetApproverEmailsAsync(SysApprovalStep step)
    {
        var emails = new List<string>();

        // 优先使用旧字段（兼容现有数据）
        if (step.ApproverUserId.HasValue)
        {
            var user = await _db.Users.FindAsync(step.ApproverUserId.Value);
            if (!string.IsNullOrEmpty(user?.Email))
            {
                emails.Add(user.Email);
            }
        }
        else if (!string.IsNullOrEmpty(step.ApproverRole))
        {
            var roleId = await _db.Roles
                .Where(r => r.RoleName == step.ApproverRole)
                .Select(r => (long?)r.Id)
                .FirstOrDefaultAsync();

            if (roleId.HasValue)
            {
                var userIds = await _db.UserRoles
                    .Where(ur => ur.RoleId == roleId.Value)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                var roleEmails = await _db.Users
                    .Where(u => userIds.Contains(u.Id) && !string.IsNullOrEmpty(u.Email))
                    .Select(u => u.Email!)
                    .ToListAsync();

                emails.AddRange(roleEmails);
            }
        }
        // 兼容 Rules 体系：从解析规则中获取审批人
        else if (step.Rules.Any(r => r.IsActive == true))
        {
            foreach (var rule in step.Rules.Where(r => r.IsActive == true).OrderBy(r => r.Priority))
            {
                if (rule.RuleType == "FIXED_USER" && long.TryParse(rule.RuleValue, out var userId))
                {
                    var user = await _db.Users.FindAsync(userId);
                    if (!string.IsNullOrEmpty(user?.Email))
                    {
                        emails.Add(user.Email);
                    }
                }
                else if (rule.RuleType == "FIXED_ROLE" && !string.IsNullOrEmpty(rule.RuleValue))
                {
                    var roleId = await _db.Roles
                        .Where(r => r.RoleName == rule.RuleValue)
                        .Select(r => (long?)r.Id)
                        .FirstOrDefaultAsync();

                    if (roleId.HasValue)
                    {
                        var userIds = await _db.UserRoles
                            .Where(ur => ur.RoleId == roleId.Value)
                            .Select(ur => ur.UserId)
                            .ToListAsync();

                        var roleEmails = await _db.Users
                            .Where(u => userIds.Contains(u.Id) && !string.IsNullOrEmpty(u.Email))
                            .Select(u => u.Email!)
                            .ToListAsync();

                        emails.AddRange(roleEmails);
                    }
                }
                else if (rule.RuleType == "ORG_TREE" && !string.IsNullOrEmpty(rule.RuleValue))
                {
                    // ORG_TREE 解析到具体人，需 ResolveAsync 才能确定，此处简化：如果 Fallback ROLE 则发送给该角色
                    if (!string.IsNullOrEmpty(rule.Fallback))
                    {
                        var fbParts = rule.Fallback.Split(':', 2);
                        if (fbParts.Length == 2 && fbParts[0] == "ROLE")
                        {
                            var roleId = await _db.Roles
                                .Where(r => r.RoleName == fbParts[1])
                                .Select(r => (long?)r.Id)
                                .FirstOrDefaultAsync();

                            if (roleId.HasValue)
                            {
                                var userIds = await _db.UserRoles
                                    .Where(ur => ur.RoleId == roleId.Value)
                                    .Select(ur => ur.UserId)
                                    .ToListAsync();

                                var roleEmails = await _db.Users
                                    .Where(u => userIds.Contains(u.Id) && !string.IsNullOrEmpty(u.Email))
                                    .Select(u => u.Email!)
                                    .ToListAsync();

                                emails.AddRange(roleEmails);
                            }
                        }
                    }
                }
            }
        }

        return emails.Distinct().ToList();
    }

    /// <summary>
    /// 解析下一步骤：优先评估条件分支，无匹配则按顺序推进
    /// </summary>
    private async Task<SysApprovalStep?> ResolveNextStepAsync(SysApprovalInstance instance, SysApprovalStep currentStep, Dictionary<string, string> variables)
    {
        var steps = instance.Template?.Steps
            .Where(s => s.IsActive == true)
            .OrderBy(s => s.StepOrder)
            .ToList() ?? new List<SysApprovalStep>();

        // 1. 评估条件分支（按优先级排序）
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
                        return targetStep;
                    }
                }
            }
        }

        // 2. 无匹配条件，按默认顺序推进
        return steps
            .Where(s => s.StepOrder > currentStep.StepOrder)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefault();
    }

    /// <summary>
    /// 评估简单条件表达式：支持 Field &gt;/&gt;=/&lt;=/==/!= Value
    /// 数值类型自动解析，字符串需用单引号包裹
    /// </summary>
    private static bool EvaluateCondition(string? expression, Dictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(expression)) return false;

        // 解析表达式：FieldName Operator Value
        // 支持的运算符：> < >= <= == !=
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

        // 尝试数值比较
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

        // 字符串比较（去除单引号）
        var compareValue = valueStr.Trim('\'');
        return foundOp switch
        {
            "==" => string.Equals(fieldValue, compareValue, StringComparison.OrdinalIgnoreCase),
            "!=" => !string.Equals(fieldValue, compareValue, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    #region 映射方法

    private static ApprovalTemplateDto MapToDto(SysApprovalTemplate t)
    {
        return new ApprovalTemplateDto
        {
            Id = t.Id,
            TemplateCode = t.TemplateCode,
            TemplateName = t.TemplateName,
            ModuleType = t.ModuleType,
            Description = t.Description,
            IsDefault = t.IsDefault,
            IsActive = t.IsActive,
            Site = t.Site,
            Steps = t.Steps.Where(s => s.IsActive == true).Select(s => new ApprovalStepConfigDto
            {
                Id = s.Id,
                StepName = s.StepName,
                StepOrder = s.StepOrder,
                StepType = s.StepType,
                StepMode = s.StepMode,
                ApproverRole = s.ApproverRole,
                ApproverUserId = s.ApproverUserId,
                CanReject = s.CanReject,
                CanTransfer = s.CanTransfer,
                NotifyEmailTemplate = s.NotifyEmailTemplate,
                RejectBehavior = s.RejectBehavior,
                RejectTargetStepId = s.RejectTargetStepId,
                TimeoutHours = s.TimeoutHours,
                Rules = s.Rules.Where(r => r.IsActive == true).Select(r => new ApprovalRuleDto
                {
                    Id = r.Id,
                    RuleType = r.RuleType,
                    RuleValue = r.RuleValue,
                    Fallback = r.Fallback,
                    Priority = r.Priority,
                    IsActive = r.IsActive
                }).ToList(),
                Conditions = s.Conditions.Where(c => c.IsActive == true).Select(c => new ApprovalConditionDto
                {
                    Id = c.Id,
                    ConditionName = c.ConditionName,
                    Expression = c.Expression,
                    TargetStepId = c.TargetStepId,
                    Priority = c.Priority,
                    IsActive = c.IsActive
                }).ToList()
            }).ToList()
        };
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

    #endregion

    #endregion
}
