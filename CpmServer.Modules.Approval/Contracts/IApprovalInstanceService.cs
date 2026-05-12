using CpmServer.Models;
using CpmServer.Modules.Approval.DTOs;

namespace CpmServer.Modules.Approval.Contracts;

public interface IApprovalInstanceService
{
    Task<SysApprovalInstance> StartApprovalAsync(string businessType, long businessId, string templateCode);
    Task<SysApprovalInstance> StartApprovalAsync(string businessType, long businessId, string moduleType, string? site, long submitterId);
    Task<SysApprovalInstance?> GetInstanceAsync(string businessType, long businessId);
    Task<List<ApprovalForecastStepDto>> ForecastApprovalAsync(string moduleType, string businessType, long businessId, long submitterId);
    Task ScanTimeoutTasksAsync();
}
