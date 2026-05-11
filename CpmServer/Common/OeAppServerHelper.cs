using Progress.Open4GL.DynamicAPI;
using Progress.Open4GL.Proxy;

namespace CpmServer.Common;

/// <summary>
/// Progress OpenEdge AppServer 连接帮助类
/// 封装了 QAD (Progress) 远程过程调用
/// </summary>
public static class OeAppServerHelper
{
    private static string? _appServerUrl;
    private static string? _appServerName;

    /// <summary>
    /// 初始化 AppServer 连接参数（通常在应用启动时调用一次）
    /// </summary>
    public static void Initialize(string appServerUrl, string appServerName)
    {
        _appServerUrl = appServerUrl;
        _appServerName = appServerName;
    }

    /// <summary>
    /// 执行远程 Progress 4GL 过程（异步包装，带 10 秒超时）
    /// </summary>
    public static async Task ExecProcAsync(string procName, ParamArray parms, int timeoutSeconds = 10)
    {
        if (string.IsNullOrEmpty(_appServerUrl))
            throw new InvalidOperationException("AppServer URL 未初始化，请先调用 OeAppServerHelper.Initialize()");
        if (string.IsNullOrEmpty(_appServerName))
            throw new InvalidOperationException("AppServer Name 未初始化，请先调用 OeAppServerHelper.Initialize()");

        await ExecProcAsync(_appServerUrl, _appServerName, procName, parms, timeoutSeconds);
    }

    /// <summary>
    /// 执行远程 Progress 4GL 过程（异步包装，带超时控制）
    /// 防止同步连接 AppServer 阻塞主线程导致前端 CORS 超时
    /// </summary>
    public static async Task ExecProcAsync(string appServerUrl, string appServerName, string procName, ParamArray parms, int timeoutSeconds = 10)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

        await Task.Run(() =>
        {
            Connection conn = new Connection(appServerUrl, "", "", "");
            conn.SessionModel = 1;  // session-free
            try
            {
                OpenAppObject openAO = new OpenAppObject(conn, appServerName);
                openAO.RunProc(procName, parms);
                openAO.Dispose();
            }
            finally
            {
                conn.ReleaseConnection();
            }
        }, cts.Token);
    }
}
