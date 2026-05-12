using CpmServer.Models;
using CpmServer.Modules.Approval.Contracts;
using CpmServer.Modules.Approval.DTOs;

namespace CpmServer.Modules.Approval.Services;

public class ApprovalService : IApprovalService
{
    private readonly IApprovalTemplateService _templateService;
    private readonly IApprovalInstanceService _instanceService;
    private readonly IApprovalTaskService _taskService;
    private readonly IApprovalActionService _actionService;

    public ApprovalService(
        IApprovalTemplateService templateService,
        IApprovalInstanceService instanceService,
        IApprovalTaskService taskService,
        IApprovalActionService actionService)
    {
        _templateService = templateService;
        _instanceService = instanceService;
        _taskService = taskService;
        _actionService = actionService;
    }

    #region IApprovalTemplateService

    public Task<List<ApprovalTemplateDto>> GetTemplatesAsync(string? moduleType)
        => _templateService.GetTemplatesAsync(moduleType);

    public Task<ApprovalTemplateDto?> GetTemplateByIdAsync(long id)
        => _templateService.GetTemplateByIdAsync(id);

    public Task<long> CreateTemplateAsync(ApprovalTemplateDto dto)
        => _templateService.CreateTemplateAsync(dto);

    public Task UpdateTemplateAsync(long id, ApprovalTemplateDto dto)
        => _templateService.UpdateTemplateAsync(id, dto);

    public Task DeleteTemplateAsync(long id)
        => _templateService.DeleteTemplateAsync(id);

    #endregion

    #region IApprovalInstanceService

    public Task<SysApprovalInstance> StartApprovalAsync(string businessType, long businessId, string templateCode)
        => _instanceService.StartApprovalAsync(businessType, businessId, templateCode);

    public Task<SysApprovalInstance> StartApprovalAsync(string businessType, long businessId, string moduleType, string? site, long submitterId)
        => _instanceService.StartApprovalAsync(businessType, businessId, moduleType, site, submitterId);

    public Task<SysApprovalInstance?> GetInstanceAsync(string businessType, long businessId)
        => _instanceService.GetInstanceAsync(businessType, businessId);

    public Task<List<ApprovalForecastStepDto>> ForecastApprovalAsync(string moduleType, string businessType, long businessId, long submitterId)
        => _instanceService.ForecastApprovalAsync(moduleType, businessType, businessId, submitterId);

    public Task ScanTimeoutTasksAsync()
        => _instanceService.ScanTimeoutTasksAsync();

    #endregion

    #region IApprovalTaskService

    public Task<List<ApprovalTaskDto>> GetPendingTasksAsync(long userId)
        => _taskService.GetPendingTasksAsync(userId);

    public Task<List<ApprovalTaskDto>> GetInstanceTasksAsync(long instanceId)
        => _taskService.GetInstanceTasksAsync(instanceId);

    public Task<List<ApprovalRecordDto>> GetApprovalRecordsAsync(long instanceId)
        => _taskService.GetApprovalRecordsAsync(instanceId);

    public Task<List<ApprovalStepDto>> GetApprovalStepsWithStatusAsync(long instanceId)
        => _taskService.GetApprovalStepsWithStatusAsync(instanceId);

    public Task<bool> CanUserApproveAsync(long instanceId, long userId)
        => _taskService.CanUserApproveAsync(instanceId, userId);

    public Task<SysApprovalStep?> GetCurrentStepAsync(long instanceId)
        => _taskService.GetCurrentStepAsync(instanceId);

    #endregion

    #region IApprovalActionService

    public Task ApproveAsync(long instanceId, long approverId, string comment, decimal? reviewCost = null)
        => _actionService.ApproveAsync(instanceId, approverId, comment, reviewCost);

    public Task RejectAsync(long instanceId, long approverId, string comment)
        => _actionService.RejectAsync(instanceId, approverId, comment);

    public Task TransferAsync(long instanceId, long fromUserId, long toUserId, string? comment)
        => _actionService.TransferAsync(instanceId, fromUserId, toUserId, comment);

    #endregion
}
