using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using CpmServer.Modules.Approval.DTOs;
using CpmServer.Services;
using Microsoft.EntityFrameworkCore;

namespace CpmServer.Modules.Approval.Services;

public class ApprovalTemplateService : IApprovalTemplateService
{
    private readonly CpmDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IGeneralizedCodeService _gc;

    public ApprovalTemplateService(CpmDbContext db, ICurrentUser currentUser, IGeneralizedCodeService gc)
    {
        _db = db;
        _currentUser = currentUser;
        _gc = gc;
    }

    public async Task<List<ApprovalTemplateDto>> GetTemplatesAsync(string? moduleType)
    {
        var currentApp = _currentUser.App ?? "cpm";
        var currentSite = _currentUser.Site;

        var query = _db.ApprovalTemplates
            .Include(t => t.Steps)
            .ThenInclude(s => s.Rules)
            .Include(t => t.Steps)
            .ThenInclude(s => s.Conditions)
            .Where(t => (t.App == currentApp || string.IsNullOrEmpty(t.App)) &&
                        (t.Site == currentSite || string.IsNullOrEmpty(t.Site)))
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
        var currentApp = _currentUser.App ?? "cpm";
        var currentSite = !string.IsNullOrWhiteSpace(dto.Site) ? dto.Site : _currentUser.Site;

        foreach (var step in dto.Steps)
        {
            await _gc.ValidateAsync("APPROVAL_STEP_TYPE", step.StepType, currentSite, currentApp);
            await _gc.ValidateAsync("APPROVAL_STEP_MODE", step.StepMode, currentSite, currentApp);
            if (!string.IsNullOrWhiteSpace(step.RejectBehavior))
                await _gc.ValidateAsync("REJECT_BEHAVIOR", step.RejectBehavior, currentSite, currentApp);
        }

        var entity = new SysApprovalTemplate
        {
            TemplateCode = GenerateTemplateCode(dto.ModuleType),
            TemplateName = dto.TemplateName,
            ModuleType = dto.ModuleType,
            Description = dto.Description,
            IsDefault = dto.IsDefault,
            IsActive = dto.IsActive,
            Site = !string.IsNullOrWhiteSpace(dto.Site) ? dto.Site : _currentUser.Site,
            App = currentApp,
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
                Site = entity.Site,
                App = currentApp,
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
                    IsActive = rule.IsActive,
                    Site = entity.Site,
                    App = currentApp
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
                    IsActive = cond.IsActive,
                    Site = entity.Site,
                    App = currentApp
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

        var currentApp = _currentUser.App ?? "cpm";
        entity.TemplateName = dto.TemplateName;
        entity.ModuleType = dto.ModuleType;
        entity.Description = dto.Description;
        entity.IsDefault = dto.IsDefault;
        entity.IsActive = dto.IsActive;
        entity.Site = !string.IsNullOrWhiteSpace(dto.Site) ? dto.Site : entity.Site;
        entity.App = currentApp;

        foreach (var step in dto.Steps)
        {
            await _gc.ValidateAsync("APPROVAL_STEP_TYPE", step.StepType, entity.Site, currentApp);
            await _gc.ValidateAsync("APPROVAL_STEP_MODE", step.StepMode, entity.Site, currentApp);
            if (!string.IsNullOrWhiteSpace(step.RejectBehavior))
                await _gc.ValidateAsync("REJECT_BEHAVIOR", step.RejectBehavior, entity.Site, currentApp);
        }

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
                            IsActive = condDto.IsActive,
                            Site = entity.Site,
                            App = currentApp
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
                    TimeoutHours = stepDto.TimeoutHours,
                    Site = entity.Site,
                    App = currentApp
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
                        IsActive = ruleDto.IsActive,
                        Site = entity.Site,
                        App = currentApp
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
                        IsActive = condDto.IsActive,
                        Site = entity.Site,
                        App = currentApp
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
}
