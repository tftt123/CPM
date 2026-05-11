using CpmServer.Models;

namespace CpmServer.Modules.Approval.Contracts;

/// <summary>
/// 业务变量提供者 —— 由业务模块实现，为审批引擎提供邮件模板变量
/// </summary>
public interface IBusinessVariableProvider
{
    /// <summary>是否支持该业务类型</summary>
    bool Supports(string businessType);

    /// <summary>获取业务变量字典（用于邮件模板替换）</summary>
    Task<Dictionary<string, string>> GetVariablesAsync(long businessId);

    /// <summary>获取业务单据创建人的邮箱</summary>
    Task<string?> GetCreatorEmailAsync(long businessId);

    /// <summary>获取业务单据的 Site</summary>
    Task<string?> GetBusinessSiteAsync(long businessId);
}
