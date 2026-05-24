using CpmServer.Common;
using CpmServer.Data;
using CpmServer.Models;
using CpmServer.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace CpmServer.Services;

public class EmailService : IEmailService
{
    private readonly CpmDbContext _db;
    private readonly ILogger<EmailService> _logger;
    private readonly ICurrentUser _currentUser;

    public EmailService(CpmDbContext db, ILogger<EmailService> logger, ICurrentUser currentUser)
    {
        _db = db;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task SendEmailAsync(string toAddress, string subject, string body)
    {
        var config = await GetActiveConfigAsync();
        if (config == null)
        {
            _logger.LogWarning("邮件配置不存在，无法发送邮件到 {To}", toAddress);
            await SaveEmailLogAsync(toAddress, subject, body, 2, "邮件配置不存在");
            throw new BusinessException("邮件配置不存在，请先配置 SMTP");
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(config.FromName ?? "CPM系统", config.FromAddress));
            message.To.Add(MailboxAddress.Parse(toAddress));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            // 10 秒连接超时
            client.Timeout = 10000;

            var sslOptions = config.EnableSsl
                ? SecureSocketOptions.SslOnConnect  // 465 端口隐式 SSL
                : SecureSocketOptions.None;

            await client.ConnectAsync(config.SmtpServer, config.SmtpPort, sslOptions);
            await client.AuthenticateAsync(config.SmtpUsername, config.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("邮件发送成功: {To}, 主题: {Subject}", toAddress, subject);
            await SaveEmailLogAsync(toAddress, subject, body, 1, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "邮件发送失败: {To}, 主题: {Subject}", toAddress, subject);
            await SaveEmailLogAsync(toAddress, subject, body, 2, ex.Message);
            throw new BusinessException($"邮件发送失败: {ex.Message}");
        }
    }

    public async Task SendEmailByTemplateAsync(string templateCode, Dictionary<string, string> variables, string toAddress)
    {
        var templateQuery = _db.EmailTemplates
            .Where(t => t.TemplateCode == templateCode && t.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            templateQuery = templateQuery.Where(t => t.Site == _currentUser.Site);
        }

        var template = await templateQuery.FirstOrDefaultAsync();

        if (template == null)
        {
            _logger.LogWarning("邮件模板不存在: {TemplateCode}", templateCode);
            return;
        }

        var subject = template.Subject;
        var body = template.Body;

        foreach (var variable in variables)
        {
            subject = subject.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            body = body.Replace($"{{{{{variable.Key}}}}}", variable.Value);
        }

        await SendEmailAsync(toAddress, subject, body);
    }

    public async Task<EmailConfigDto?> GetActiveConfigAsync()
    {
        var query = _db.EmailConfigs
            .Where(c => c.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(c => c.Site == _currentUser.Site);
        }

        var config = await query
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync();

        if (config == null) return null;

        return new EmailConfigDto
        {
            Id = config.Id,
            SmtpServer = config.SmtpServer,
            SmtpPort = config.SmtpPort,
            SmtpUsername = config.SmtpUsername,
            SmtpPassword = config.SmtpPassword,
            FromName = config.FromName,
            FromAddress = config.FromAddress,
            EnableSsl = config.EnableSsl,
            IsActive = config.IsActive
        };
    }

    public async Task SaveConfigAsync(EmailConfigDto dto)
    {
        if (dto.Id.HasValue)
        {
            var entity = await _db.EmailConfigs.FindAsync(dto.Id.Value);
            if (entity != null)
            {
                entity.SmtpServer = dto.SmtpServer;
                entity.SmtpPort = dto.SmtpPort;
                entity.SmtpUsername = dto.SmtpUsername;
                entity.SmtpPassword = dto.SmtpPassword;
                entity.FromName = dto.FromName;
                entity.FromAddress = dto.FromAddress;
                entity.EnableSsl = dto.EnableSsl;
                entity.IsActive = dto.IsActive;
            }
        }
        else
        {
            _db.EmailConfigs.Add(new SysEmailConfig
            {
                SmtpServer = dto.SmtpServer,
                SmtpPort = dto.SmtpPort,
                SmtpUsername = dto.SmtpUsername,
                SmtpPassword = dto.SmtpPassword,
                FromName = dto.FromName,
                FromAddress = dto.FromAddress,
                EnableSsl = dto.EnableSsl,
                IsActive = dto.IsActive,
                Site = _currentUser.Site
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<EmailTemplateDto>> GetTemplatesAsync()
    {
        var query = _db.EmailTemplates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(_currentUser.Site))
        {
            query = query.Where(t => t.Site == _currentUser.Site);
        }

        return await query
            .OrderBy(t => t.TemplateCode)
            .Select(t => new EmailTemplateDto
            {
                Id = t.Id,
                TemplateCode = t.TemplateCode,
                TemplateName = t.TemplateName,
                Subject = t.Subject,
                Body = t.Body,
                IsSystem = t.IsSystem,
                IsActive = t.IsActive
            })
            .ToListAsync();
    }

    public async Task UpdateTemplateAsync(long id, EmailTemplateDto dto)
    {
        var entity = await _db.EmailTemplates.FindAsync(id);
        if (entity == null) return;

        entity.TemplateName = dto.TemplateName;
        entity.Subject = dto.Subject;
        entity.Body = dto.Body;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
    }

    public async Task SendTestEmailAsync(string toAddress, string subject, string body)
    {
        await SendEmailAsync(toAddress, subject, body);
    }

    private async Task SaveEmailLogAsync(string to, string subject, string body, int status, string? error)
    {
        _db.EmailLogs.Add(new SysEmailLog
        {
            ToAddress = to,
            Subject = subject,
            Body = body.Length > 2000 ? body[..2000] : body,
            Status = status,
            ErrorMessage = error,
            SentAt = status == 1 ? DateTime.Now : null,
            Site = _currentUser.Site
        });
        await _db.SaveChangesAsync();
    }
}
