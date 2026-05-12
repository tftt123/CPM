using CpmServer.Models;
using CpmServer.Modules.Approval.DTOs;

namespace CpmServer.Modules.Approval.Contracts;

public interface IApprovalTaskService
{
    Task<List<ApprovalTaskDto>> GetPendingTasksAsync(long userId);
    Task<List<ApprovalTaskDto>> GetInstanceTasksAsync(long instanceId);
    Task<List<ApprovalRecordDto>> GetApprovalRecordsAsync(long instanceId);
    Task<List<ApprovalStepDto>> GetApprovalStepsWithStatusAsync(long instanceId);
    Task<bool> CanUserApproveAsync(long instanceId, long userId);
    Task<SysApprovalStep?> GetCurrentStepAsync(long instanceId);
}
