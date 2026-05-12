using CpmServer.Modules.Approval.DTOs;

namespace CpmServer.Modules.Approval.Contracts;

public interface IApprovalTemplateService
{
    Task<List<ApprovalTemplateDto>> GetTemplatesAsync(string? moduleType);
    Task<ApprovalTemplateDto?> GetTemplateByIdAsync(long id);
    Task<long> CreateTemplateAsync(ApprovalTemplateDto dto);
    Task UpdateTemplateAsync(long id, ApprovalTemplateDto dto);
    Task DeleteTemplateAsync(long id);
}
