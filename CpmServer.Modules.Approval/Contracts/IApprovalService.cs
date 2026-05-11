using CpmServer.Models;
using CpmServer.Modules.Approval.DTOs;

namespace CpmServer.Modules.Approval.Contracts;

public interface IApprovalService
{
    // 审批模板管理
    Task<List<ApprovalTemplateDto>> GetTemplatesAsync(string? moduleType);
    Task<ApprovalTemplateDto?> GetTemplateByIdAsync(long id);
    Task<long> CreateTemplateAsync(ApprovalTemplateDto dto);
    Task UpdateTemplateAsync(long id, ApprovalTemplateDto dto);
    Task DeleteTemplateAsync(long id);

    // 审批流程执行
    Task<SysApprovalInstance> StartApprovalAsync(string businessType, long businessId, string templateCode);
    Task<SysApprovalInstance> StartApprovalAsync(string businessType, long businessId, string moduleType, string? site, long submitterId);
    Task<SysApprovalInstance?> GetInstanceAsync(string businessType, long businessId);
    Task<List<ApprovalRecordDto>> GetApprovalRecordsAsync(long instanceId);
    Task<List<ApprovalStepDto>> GetApprovalStepsWithStatusAsync(long instanceId);

    // 待办任务查询
    Task<List<ApprovalTaskDto>> GetPendingTasksAsync(long userId);
    Task<List<ApprovalTaskDto>> GetInstanceTasksAsync(long instanceId);

    // 审批操作
    Task ApproveAsync(long instanceId, long approverId, string comment, decimal? reviewCost = null);
    Task RejectAsync(long instanceId, long approverId, string comment);
    Task TransferAsync(long instanceId, long fromUserId, long toUserId, string? comment);
    Task<bool> CanUserApproveAsync(long instanceId, long userId);
    Task<SysApprovalStep?> GetCurrentStepAsync(long instanceId);

    // 审批预测（不创建实例，仅预览路线）
    Task<List<ApprovalForecastStepDto>> ForecastApprovalAsync(string moduleType, string businessType, long businessId, long submitterId);

    // 超时扫描
    Task ScanTimeoutTasksAsync();
}
