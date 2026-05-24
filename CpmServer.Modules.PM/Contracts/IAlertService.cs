using CpmServer.Models;

namespace CpmServer.Modules.PM.Contracts;

/// <summary>
/// 报警服务接口
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// 发送实际节拍超报价节拍报警
    /// </summary>
    Task SendCycleTimeExceededAlertAsync(
        PmProjectTraceStep step,
        PmProjectTraceStepActualCycleTime actualRecord,
        string? submitterName);
}
