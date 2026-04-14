using System.Collections.Concurrent;
using System.Diagnostics;
using EmbedIO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA2;
using FlaUI.UIA3;

namespace Swg.FlaUI;

/// <summary>
/// 会话、应用与窗口相关 API；并承载跨区共享的会话解析与枚举解析等 internal 辅助。
/// </summary>
public static class SwgFlaUISessionApplication
{
    internal static readonly ConcurrentDictionary<string, SessionState> Sessions = new();

    /// <summary>
    /// 创建自动化会话（Session），按可执行路径附加或按策略启动目标应用。
    /// </summary>
    public static SessionCreateResult CreateSession(SessionCreateSpec request)
    {
        // 中文注释：统一按可执行路径查找进程；仅在显式允许时才启动新进程。
        if (string.IsNullOrWhiteSpace(request.ExecutablePath))
        {
            throw HttpException.BadRequest("executablePath is required.");
        }

        var automation = CreateAutomation(request.AutomationType);
        var application = AttachOrLaunchByPath(request.ExecutablePath!, request.Arguments, request.LaunchIfNotRunning, request.ProcessIndex);

        var sessionId = Guid.NewGuid().ToString("N");
        var session = new SessionState(sessionId, automation, application);
        Sessions[sessionId] = session;
        return new SessionCreateResult(sessionId, application.ProcessId, automation.AutomationType.ToString());
    }

    /// <summary>
    /// 删除会话并释放会话持有的应用与自动化资源。
    /// </summary>
    public static void DeleteSession(string sessionId)
    {
        if (!Sessions.TryRemove(sessionId, out var session))
        {
            throw HttpException.NotFound("Session not found.");
        }

        session.Dispose();
    }

    /// <summary>
    /// 关闭会话关联的应用进程。
    /// </summary>
    /// <returns>是否已成功关闭。</returns>
    public static bool CloseApplication(string sessionId, CloseApplicationSpec request)
    {
        var session = ResolveSession(sessionId);
        return session.Application.Close(request.KillIfCloseFails);
    }

    /// <summary>
    /// 强制终止会话关联的应用进程。
    /// </summary>
    public static void KillApplication(string sessionId)
    {
        ResolveSession(sessionId).Application.Kill();
    }

    /// <summary>
    /// 等待应用退出忙碌状态。
    /// </summary>
    /// <returns>WaitWhileBusy 的布尔结果。</returns>
    public static bool WaitWhileBusy(string sessionId, WaitTimeoutSpec request)
    {
        var session = ResolveSession(sessionId);
        return session.Application.WaitWhileBusy(ToNullableTimeout(request.TimeoutMs));
    }

    /// <summary>
    /// 等待应用主窗口句柄可用。
    /// </summary>
    /// <returns>WaitWhileMainHandleIsMissing 的布尔结果。</returns>
    public static bool WaitMainHandle(string sessionId, WaitTimeoutSpec request)
    {
        var session = ResolveSession(sessionId);
        return session.Application.WaitWhileMainHandleIsMissing(ToNullableTimeout(request.TimeoutMs));
    }

    /// <summary>
    /// 获取应用主窗口并注册为 elementId。
    /// </summary>
    public static ElementRefResult GetMainWindow(string sessionId, WaitTimeoutSpec request)
    {
        var session = ResolveSession(sessionId);
        var window = session.Application.GetMainWindow(session.Automation, ToNullableTimeout(request.TimeoutMs));
        if (window == null)
        {
            throw HttpException.NotFound("Main window not found.");
        }
        return new ElementRefResult(session.AddElement(window));
    }

    /// <summary>
    /// 获取应用所有顶层窗口并注册为 elementId 列表。
    /// </summary>
    public static IReadOnlyList<ElementRefResult> GetTopLevelWindows(string sessionId)
    {
        var session = ResolveSession(sessionId);
        return session.Application.GetAllTopLevelWindows(session.Automation)
            .Select(session.AddElement)
            .Select(x => new ElementRefResult(x))
            .ToArray();
    }

    internal static TimeSpan? ToNullableTimeout(int? timeoutMs)
    {
        return timeoutMs.HasValue ? TimeSpan.FromMilliseconds(timeoutMs.Value) : null;
    }

    internal static string? AddNullableElement(SessionState session, AutomationElement? element)
    {
        return element == null ? null : session.AddElement(element);
    }

    internal static SessionState ResolveSession(string sessionId)
    {
        if (!Sessions.TryGetValue(sessionId, out var session))
        {
            throw HttpException.NotFound("Session not found.");
        }

        return session;
    }

    internal static AutomationElement ResolveElement(string sessionId, string elementId)
    {
        return ResolveElement(ResolveSession(sessionId), elementId);
    }

    internal static AutomationElement ResolveElement(SessionState session, string elementId)
    {
        var element = session.GetElement(elementId);
        if (!element.IsAvailable)
        {
            throw HttpException.NotFound("Element is not available anymore.");
        }

        return element;
    }

    internal static TEnum ParseEnumOrBadRequest<TEnum>(string? value, string fieldName) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw HttpException.BadRequest($"Missing {fieldName}. Valid values: {string.Join(", ", Enum.GetNames<TEnum>())}");
        }

        if (Enum.TryParse<TEnum>(value, true, out var parsed) && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw HttpException.BadRequest($"Invalid {fieldName}: {value}. Valid values: {string.Join(", ", Enum.GetNames<TEnum>())}");
    }

    private static Application AttachOrLaunchByPath(string executablePath, string? arguments, bool launchIfNotRunning, int processIndex)
    {
        var processName = Path.GetFileNameWithoutExtension(executablePath);
        if (string.IsNullOrWhiteSpace(processName))
        {
            throw HttpException.BadRequest("Invalid executablePath.");
        }

        var candidates = Process.GetProcessesByName(processName)
            .Where(p =>
            {
                try
                {
                    // 中文注释：优先按完整路径匹配，避免同名进程误附加。
                    return string.Equals(p.MainModule?.FileName, executablePath, StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            })
            .ToArray();

        if (candidates.Length > 0)
        {
            if (processIndex < 0 || processIndex >= candidates.Length)
            {
                throw HttpException.BadRequest($"processIndex out of range. matched count: {candidates.Length}");
            }

            return Application.Attach(candidates[processIndex]);
        }

        if (!launchIfNotRunning)
        {
            throw HttpException.BadRequest("Process is not running. Set launchIfNotRunning=true to launch it.");
        }

        return Application.Launch(executablePath, arguments ?? string.Empty);
    }

    private static AutomationBase CreateAutomation(string? automationType)
    {
        if (string.IsNullOrWhiteSpace(automationType))
        {
            return new UIA3Automation();
        }

        var parsed = ParseEnumOrBadRequest<AutomationType>(automationType, nameof(automationType));
        if (parsed == AutomationType.UIA2)
        {
            return new UIA2Automation();
        }

        return new UIA3Automation();
    }
}
