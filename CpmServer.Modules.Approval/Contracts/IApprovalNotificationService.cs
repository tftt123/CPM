using CpmServer.Models;

namespace CpmServer.Modules.Approval.Contracts;

public interface IApprovalNotificationService
{
    Task NotifyStepAsync(SysApprovalInstance instance, SysApprovalStep step, string businessType, long businessId);
    Task NotifyCompleteAsync(SysApprovalInstance instance, string businessType, long businessId, bool approved);
    Task<List<string>> GetApproverEmailsAsync(SysApprovalStep step);
}
