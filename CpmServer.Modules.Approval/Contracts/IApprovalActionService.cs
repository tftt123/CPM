namespace CpmServer.Modules.Approval.Contracts;

public interface IApprovalActionService
{
    Task ApproveAsync(long instanceId, long approverId, string comment, decimal? reviewCost = null);
    Task RejectAsync(long instanceId, long approverId, string comment);
    Task TransferAsync(long instanceId, long fromUserId, long toUserId, string? comment);
}
