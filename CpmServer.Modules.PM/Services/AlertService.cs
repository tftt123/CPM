using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Modules.PM.Contracts;
using CpmServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CpmServer.Modules.PM.Services;

public class AlertService : IAlertService
{
    private readonly CpmDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<AlertService> _logger;
    private readonly ICurrentUser _currentUser;

    public AlertService(CpmDbContext db, IEmailService emailService, ILogger<AlertService> logger, ICurrentUser currentUser)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task SendCycleTimeExceededAlertAsync(
        PmProjectTraceStep step,
        PmProjectTraceStepActualCycleTime actualRecord,
        string? submitterName)
    {
        var recipients = await GetRecipientsAsync("CYCLE_TIME_EXCEEDED");
        if (recipients.Count == 0)
        {
            _logger.LogWarning("未配置 CYCLE_TIME_EXCEEDED 报警收件人");
            return;
        }

        var variables = BuildVariables(step, actualRecord, submitterName);

        foreach (var email in recipients)
        {
            try
            {
                await _emailService.SendEmailByTemplateAsync("CYCLE_TIME_EXCEEDED", variables, email);
                _logger.LogInformation("节拍超标报警邮件已发送: {Email}, 工序: {ProcessName}", email, step.ProcessName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "节拍超标报警邮件发送失败: {Email}, 工序: {ProcessName}", email, step.ProcessName);
            }
        }
    }

    private async Task<List<string>> GetRecipientsAsync(string alertType)
    {
        var query = _db.AlertRecipients
            .Where(r => r.AlertType == alertType && r.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(r => r.Site == _currentUser.Site);
        }

        var configs = await query.ToListAsync();

        var emails = new List<string>();

        foreach (var config in configs)
        {
            switch (config.RecipientType)
            {
                case "EMAIL":
                    emails.Add(config.RecipientValue);
                    break;

                case "ROLE":
                    {
                        var roleId = await _db.Roles
                            .Where(r => r.RoleName == config.RecipientValue)
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
                    break;
            }
        }

        return emails.Distinct().ToList();
    }

    private static Dictionary<string, string> BuildVariables(
        PmProjectTraceStep step,
        PmProjectTraceStepActualCycleTime actualRecord,
        string? submitterName)
    {
        var quoted = step.CycleTime ?? 0;
        var actual = actualRecord.ActualCycleTime ?? 0;
        var ratio = quoted > 0 ? ((actual - quoted) / quoted * 100).ToString("F2") : "N/A";

        return new Dictionary<string, string>
        {
            ["CustomerName"] = step.ProjectTrace?.CustomerName ?? "-",
            ["ProductCode"] = step.ProjectTrace?.ProductCode ?? "-",
            ["ProductName"] = step.ProjectTrace?.ProductName ?? "-",
            ["ProcessName"] = step.ProcessName,
            ["CycleTime"] = quoted.ToString("F4"),
            ["ActualCycleTime"] = actual.ToString("F4"),
            ["ExceedRatio"] = $"{ratio}%",
            ["RecordDate"] = actualRecord.RecordDate.ToString("yyyy-MM-dd"),
            ["SubmitterName"] = submitterName ?? "-",
            ["PersonInCharge"] = step.PersonInCharge ?? "-"
        };
    }
}
