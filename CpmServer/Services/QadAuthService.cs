using System.Diagnostics;
using System.Text.Json;

namespace CpmServer.Services;

/// <summary>
/// QAD (Progress OpenEdge) 认证服务实现
/// 通过 .NET Framework 4.8 桥接程序调用 QAD AppServer
/// 参数通过 stdin 传递，避免命令行转义问题
/// </summary>
public class QadAuthService : IQadAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<QadAuthService> _logger;
    private readonly string _bridgeExePath;

    public QadAuthService(IConfiguration configuration, ILogger<QadAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // 查找桥接程序路径
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var solutionDir = baseDir;
        while (!string.IsNullOrEmpty(solutionDir) && !Directory.Exists(Path.Combine(solutionDir, "QadAuthBridge")))
        {
            solutionDir = Directory.GetParent(solutionDir)?.FullName;
        }

        if (solutionDir != null)
        {
            var x86Path = Path.Combine(solutionDir, "QadAuthBridge", "bin", "x86", "Debug", "QadAuthBridge.exe");
            var debugPath = Path.Combine(solutionDir, "QadAuthBridge", "bin", "Debug", "QadAuthBridge.exe");
            _bridgeExePath = File.Exists(x86Path) ? x86Path : debugPath;
        }
        else
        {
            _bridgeExePath = Path.Combine(baseDir, "QadAuthBridge.exe");
        }

        _logger.LogInformation("桥接程序路径: {Path}", _bridgeExePath);
    }

    /// <inheritdoc />
    public async Task<(string Message, bool IsSuccess)> ValidateLoginAsync(string userId, string password, string domain)
    {
        var appServerUrl = _configuration["QAD:AppServerUrl"]!;
        var appServerName = _configuration["QAD:AppServerName"]!;
        var procPath = _configuration["QAD:LoginProcPath"] ?? "us/xx/xxchecklogina.p";

        try
        {
            _logger.LogInformation("开始 QAD 登录验证（桥接）- UserId: {UserId}, Domain: {Domain}",
                userId, domain);

            if (!File.Exists(_bridgeExePath))
            {
                _logger.LogError("桥接程序不存在: {Path}", _bridgeExePath);
                return ("QAD 桥接程序未找到，请检查 QadAuthBridge 项目是否已编译", false);
            }

            var psi = new ProcessStartInfo
            {
                FileName = _bridgeExePath,
                Arguments = "--stdin",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            // 通过 stdin 逐行写入参数，避免命令行转义问题
            await process.StandardInput.WriteLineAsync(userId);
            await process.StandardInput.WriteLineAsync(password);
            await process.StandardInput.WriteLineAsync(domain ?? string.Empty);
            await process.StandardInput.WriteLineAsync(appServerUrl);
            await process.StandardInput.WriteLineAsync(appServerName);
            await process.StandardInput.WriteLineAsync(procPath);
            process.StandardInput.Close();

            // 异步并发读取 stdout/stderr，防止缓冲区满导致死锁
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            // 带超时等待进程退出（30秒）
            var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(); } catch { }
                _logger.LogError("QAD 桥接程序执行超时");
                return ("QAD 验证超时，请检查 AppServer 状态", false);
            }

            string output = await outputTask;
            string error = await errorTask;

            _logger.LogInformation("桥接程序退出码: {ExitCode}, 输出: {Output}, 错误: {Error}",
                process.ExitCode, output, error);

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("桥接程序 stderr: {Error}", error);
            }

            var result = JsonSerializer.Deserialize<BridgeResult>(output.Trim());
            if (result != null)
            {
                _logger.LogInformation("QAD 验证结果 - success={Success}, message={Message}",
                    result.success, result.message);
                return (result.message ?? string.Empty, result.success);
            }

            return ($"桥接程序返回无效数据: {output}", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QAD 登录验证异常（桥接）- UserId: {UserId}", userId);
            return ($"ServiceException, please contact the admin \r\n{ex.Message}", false);
        }
    }

    private class BridgeResult
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
    }
}
