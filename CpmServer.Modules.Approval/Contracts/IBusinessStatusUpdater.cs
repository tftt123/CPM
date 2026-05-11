namespace CpmServer.Modules.Approval.Contracts;

/// <summary>
/// 业务状态更新器 —— 由业务模块实现，审批引擎在完成/驳回时回调更新业务单据状态
/// </summary>
public interface IBusinessStatusUpdater
{
    /// <summary>是否支持该业务类型</summary>
    bool Supports(string businessType);

    /// <summary>
    /// 更新业务单据状态
    /// </summary>
    /// <param name="businessId">业务单据ID</param>
    /// <param name="approvalStatus">审批引擎状态: 0=进行中, 1=完成, 2=驳回</param>
    /// <param name="currentStepId">当前步骤ID</param>
    /// <param name="reviewCost">评审成本（技术评审环节传入）</param>
    Task UpdateStatusAsync(long businessId, int approvalStatus, long? currentStepId, decimal? reviewCost = null);
}
