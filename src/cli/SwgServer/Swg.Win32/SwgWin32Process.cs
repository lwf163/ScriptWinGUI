using System.Diagnostics;

namespace Swg.Win32;

/// <summary>
/// 进程相关静态业务函数。
/// </summary>
public static class SwgWin32Process
{
    public static uint GetCurrentProcessId() => (uint)Environment.ProcessId;

    public static bool Exists(uint processId)
    {
        try
        {
            _ = Process.GetProcessById(checked((int)processId));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static uint StartProcess(string? executablePath, string? arguments)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
            throw new ArgumentException("ExecutablePath 必填。", nameof(executablePath));

        var psi = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = arguments ?? string.Empty,
            UseShellExecute = true,
        };

        try
        {
            using var p = Process.Start(psi);
            if (p is null)
                throw new InvalidOperationException("Process.Start 返回 null。");
            return (uint)p.Id;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            throw new InvalidOperationException("Start process failed.", ex);
        }
    }

    public static void KillProcess(uint processId)
    {
        int pid = checked((int)processId);
        Process p;
        try
        {
            p = Process.GetProcessById(pid);
        }
        catch
        {
            throw new InvalidOperationException("Process not found.");
        }

        try
        {
            p.Kill(entireProcessTree: true);
            // 这里不强制等待退出，以保持接口响应快。
        }
        catch (Exception ex) when (ex is InvalidOperationException or SystemException)
        {
            throw new InvalidOperationException("Kill process failed.", ex);
        }
    }

    public static bool WaitExit(uint processId, int? timeoutMs)
    {
        int pid = checked((int)processId);
        Process p;
        try
        {
            p = Process.GetProcessById(pid);
        }
        catch
        {
            throw new InvalidOperationException("Process not found.");
        }

        try
        {
            if (timeoutMs is null)
            {
                p.WaitForExit();
                return true;
            }

            if (timeoutMs < 0)
                throw new ArgumentException("TimeoutMs 必须为非负，或为 null 表示无限等待。", nameof(timeoutMs));

            return p.WaitForExit(timeoutMs.Value);
        }
        finally
        {
            // Process 不持有额外资源句柄（在 .NET 中），这里不强制 Dispose，避免影响后续外部使用。
        }
    }
}

