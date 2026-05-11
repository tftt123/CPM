namespace CpmServer.Services;

/// <summary>
/// 邮件服务接口 —— 已迁移到 SharedKernel，供所有模块共享
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string toAddress, string subject, string body);
    Task SendEmailByTemplateAsync(string templateCode, Dictionary<string, string> variables, string toAddress);
    Task<EmailConfigDto?> GetActiveConfigAsync();
    Task SaveConfigAsync(EmailConfigDto dto);
    Task SendTestEmailAsync(string toAddress, string subject, string body);
    Task<List<EmailTemplateDto>> GetTemplatesAsync();
    Task UpdateTemplateAsync(long id, EmailTemplateDto dto);
}

public class EmailConfigDto
{
    public long? Id { get; set; }
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public class EmailTemplateDto
{
    public long? Id { get; set; }
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
}
