using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using CpmServer.Modules.Approval.Services.Approvers;
using CpmServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CpmServer.Modules.Approval.Services;

public class ApprovalNotificationService
{
    private readonly CpmDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<ApprovalNotificationService> _logger;
    private readonly IEnumerable<IBusinessVariableProvider> _variableProviders;
    private readonly ICurrentUser _currentUser;

    public ApprovalNotificationService(
        CpmDbContext db,
        IEmailService emailService,
        ILogger<ApprovalNotificationService> logger,
        IEnumerable<IBusinessVariableProvider> variableProviders,
        ICurrentUser currentUser)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
        _variableProviders = variableProviders;
        _currentUser = currentUser;
    }

    public async Task NotifyStepAsync(SysApprovalInstance instance, SysApprovalStep step, string businessType, long businessId)
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

    public async Task NotifyCompleteAsync(SysApprovalInstance instance, string businessType, long businessId, bool approved)
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

    public async Task<List<string>> GetApproverEmailsAsync(SysApprovalStep step)
    {
        var emails = new List<string>();

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
                .Where(r => r.RoleName == step.ApproverRole && (r.Site == _currentUser.Site || string.IsNullOrEmpty(r.Site)))
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
                        .Where(r => r.RoleName == rule.RuleValue && (r.Site == _currentUser.Site || string.IsNullOrEmpty(r.Site)))
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
                    if (!string.IsNullOrEmpty(rule.Fallback))
                    {
                        var fbParts = rule.Fallback.Split(':', 2);
                        if (fbParts.Length == 2 && fbParts[0] == "ROLE")
                        {
                            var roleId = await _db.Roles
                                .Where(r => r.RoleName == fbParts[1] && (r.Site == _currentUser.Site || string.IsNullOrEmpty(r.Site)))
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

    private async Task<Dictionary<string, string>> GetBusinessVariablesAsync(string businessType, long businessId)
    {
        var provider = _variableProviders.FirstOrDefault(p => p.Supports(businessType));
        if (provider != null)
        {
            return await provider.GetVariablesAsync(businessId);
        }
        return new Dictionary<string, string>();
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
}
