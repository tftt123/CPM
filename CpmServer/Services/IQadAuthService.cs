namespace CpmServer.Services;

/// <summary>
/// QAD (Progress OpenEdge) 认证服务接口
/// </summary>
public interface IQadAuthService
{
    /// <summary>
    /// 验证用户登录（通过 QAD AppServer 桥接程序）
    /// </summary>
    Task<(string Message, bool IsSuccess)> ValidateLoginAsync(string userId, string password, string domain);
}
