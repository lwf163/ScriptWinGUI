using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;
using EmbedIO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Identifiers;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA2;
using FlaUI.UIA3;
using static Swg.FlaUI.SwgFlaUISessionApplication;

namespace Swg.FlaUI;

/// <summary>
/// 对应 AutomationController 中 #region SwgFlaUIAutomationElement。
/// </summary>
public static class SwgFlaUIAutomationElement
{
    /// <summary>
    /// 获取元素基础信息（名称、类型、位置、可用性等）。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>元素基础信息。</returns>
    public static ElementInfoResult GetElementInfo(string sessionId, string elementId)
    {
var element = ResolveElement(sessionId, elementId);
var rect = element.BoundingRectangle;
return new ElementInfoResult(
    elementId,
    element.Name,
    element.AutomationId,
    element.ClassName,
    element.ControlType.ToString(),
    element.FrameworkType.ToString(),
    element.IsEnabled,
    element.IsOffscreen,
    element.IsAvailable,
    new RectBounds(rect.Left, rect.Top, rect.Width, rect.Height));
    }

    /// <summary>
    /// 按条件查找首个元素并返回 elementId。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="request">元素查找参数，支持 root/scope 与常见属性条件。</param>
    /// <returns>首个匹配元素的引用 ID。</returns>
    public static ElementRefResult FindElement(string sessionId, FindElementSpec request)
    {
var session = ResolveSession(sessionId);
var root = ResolveRootElement(session, request.RootKind, request.RootElementId, request.MainWindowWaitTimeoutMs);
var condition = BuildCondition(root.ConditionFactory, request);
var scope = ParseScope(request.Scope);
var found = root.FindFirst(scope, condition);
if (found == null)
{
    throw HttpException.NotFound("Element not found.");
}

var id = session.AddElement(found);
return new ElementRefResult(id);
    }

    /// <summary>
    /// 按条件查找全部元素并返回 elementId 列表。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="request">元素查找参数，支持 root/scope 与常见属性条件。</param>
    /// <returns>匹配元素引用 ID 列表。</returns>
    public static IReadOnlyList<ElementRefResult> FindAllElements(string sessionId, FindElementSpec request)
    {
var session = ResolveSession(sessionId);
var root = ResolveRootElement(session, request.RootKind, request.RootElementId, request.MainWindowWaitTimeoutMs);
var condition = BuildCondition(root.ConditionFactory, request);
var scope = ParseScope(request.Scope);
var found = root.FindAll(scope, condition);
return found.Select(x => new ElementRefResult(session.AddElement(x))).ToArray();
    }

    /// <summary>
    /// 通过 XPath 查找首个元素并返回 elementId。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="request">XPath 查找参数。</param>
    /// <returns>首个匹配元素引用 ID。</returns>
    public static ElementRefResult FindElementByXPath(string sessionId, FindByXPathSpec request)
    {
var session = ResolveSession(sessionId);
var root = ResolveRootElement(session, request.RootKind, request.RootElementId, request.MainWindowWaitTimeoutMs);
if (string.IsNullOrWhiteSpace(request.XPath))
{
    throw HttpException.BadRequest("xPath is required.");
}

var found = root.FindFirstByXPath(request.XPath);
if (found == null)
{
    throw HttpException.NotFound("Element not found.");
}

return new ElementRefResult(session.AddElement(found));
    }

    /// <summary>
    /// 通过 XPath 查找全部元素并返回 elementId 列表。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="request">XPath 查找参数。</param>
    /// <returns>匹配元素引用 ID 列表。</returns>
    public static IReadOnlyList<ElementRefResult> FindAllElementsByXPath(string sessionId, FindByXPathSpec request)
    {
var session = ResolveSession(sessionId);
var root = ResolveRootElement(session, request.RootKind, request.RootElementId, request.MainWindowWaitTimeoutMs);
if (string.IsNullOrWhiteSpace(request.XPath))
{
    throw HttpException.BadRequest("xPath is required.");
}

return root.FindAllByXPath(request.XPath)
    .Select(session.AddElement)
    .Select(x => new ElementRefResult(x))
    .ToArray();
    }

    /// <summary>
    /// 获取指定元素的直接子元素列表。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">父元素 ID。</param>
    /// <returns>子元素引用 ID 列表。</returns>
    public static IReadOnlyList<ElementRefResult> GetChildren(string sessionId, string elementId)
    {
var session = ResolveSession(sessionId);
var element = ResolveElement(session, elementId);
return element.FindAllChildren().Select(x => new ElementRefResult(session.AddElement(x))).ToArray();
    }

    /// <summary>
    /// 将焦点设置到指定元素。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>操作结果。</returns>
    public static bool Focus(string sessionId, string elementId)
    {
        ResolveElement(sessionId, elementId).Focus();
        return true;
    }

    /// <summary>
    /// 使用 Win32 原生方式设置元素焦点。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>成功则为 true。</returns>
    public static bool FocusNative(string sessionId, string elementId)
    {
        ResolveElement(sessionId, elementId).FocusNative();
        return true;
    }

    /// <summary>
    /// 将元素对应窗口置于前台。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>成功则为 true。</returns>
    public static bool SetElementForeground(string sessionId, string elementId)
    {
        ResolveElement(sessionId, elementId).SetForeground();
        return true;
    }

    /// <summary>
    /// 对指定元素执行单击操作。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">点击参数（是否移动鼠标）。</param>
    /// <returns>成功则为 true。</returns>
    public static bool Click(string sessionId, string elementId, ClickSpec? request = null)
    {
        ResolveElement(sessionId, elementId).Click(request?.MoveMouse ?? false);
        return true;
    }

    /// <summary>
    /// 对指定元素执行双击操作。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">点击参数（是否移动鼠标）。</param>
    /// <returns>成功则为 true。</returns>
    public static bool DoubleClick(string sessionId, string elementId, ClickSpec? request = null)
    {
        ResolveElement(sessionId, elementId).DoubleClick(request?.MoveMouse ?? false);
        return true;
    }

    /// <summary>
    /// 对指定元素执行右键单击操作。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">点击参数（是否移动鼠标）。</param>
    /// <returns>成功则为 true。</returns>
    public static bool RightClick(string sessionId, string elementId, ClickSpec? request = null)
    {
        ResolveElement(sessionId, elementId).RightClick(request?.MoveMouse ?? false);
        return true;
    }

    /// <summary>
    /// 对指定元素执行右键双击操作。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">点击参数（是否移动鼠标）。</param>
    /// <returns>成功则为 true。</returns>
    public static bool RightDoubleClick(string sessionId, string elementId, ClickSpec? request = null)
    {
        ResolveElement(sessionId, elementId).RightDoubleClick(request?.MoveMouse ?? false);
        return true;
    }

    /// <summary>
    /// 获取元素可点击点坐标。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>可点击点坐标。</returns>
    public static PointResult GetClickablePoint(string sessionId, string elementId)
    {
var point = ResolveElement(sessionId, elementId).GetClickablePoint();
return new PointResult(point.X, point.Y);
    }

    /// <summary>
    /// 尝试获取元素可点击点坐标。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>是否成功及坐标。</returns>
    public static TryPointResult TryGetClickablePoint(string sessionId, string elementId)
    {
var ok = ResolveElement(sessionId, elementId).TryGetClickablePoint(out var point);
return new TryPointResult(ok, point.X, point.Y);
    }

    private static TreeScope ParseScope(string? scope)
    {
        return scope?.Trim().ToLowerInvariant() switch
        {
            "children" => TreeScope.Children,
            "descendants" => TreeScope.Descendants,
            "subtree" => TreeScope.Subtree,
            _ => TreeScope.Descendants
        };
    }

    private static ConditionBase BuildCondition(ConditionFactory conditionFactory, FindElementSpec request)
    {
        if (!string.IsNullOrWhiteSpace(request.XPath))
        {
            throw HttpException.BadRequest("XPath is only supported by /find-xpath endpoints.");
        }

        var conditions = new List<ConditionBase>();
        if (!string.IsNullOrWhiteSpace(request.AutomationId))
        {
            conditions.Add(conditionFactory.ByAutomationId(request.AutomationId));
        }
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            conditions.Add(conditionFactory.ByName(request.Name));
        }
        if (!string.IsNullOrWhiteSpace(request.ClassName))
        {
            conditions.Add(conditionFactory.ByClassName(request.ClassName));
        }
        if (!string.IsNullOrWhiteSpace(request.ControlType))
        {
            var controlType = ParseEnumOrBadRequest<ControlType>(request.ControlType, nameof(request.ControlType));
            conditions.Add(conditionFactory.ByControlType(controlType));
        }

        if (conditions.Count == 0)
        {
            return TrueCondition.Default;
        }

        var merged = conditions[0];
        for (var i = 1; i < conditions.Count; i++)
        {
            merged = merged.And(conditions[i]);
        }

        return merged;
    }

    private static AutomationElement ResolveRootElement(SessionState session, string? rootKind, string? rootElementId, int? mainWindowWaitTimeoutMs)
    {
        if (!string.IsNullOrWhiteSpace(rootElementId))
        {
            return ResolveElement(session, rootElementId);
        }

        if (string.Equals(rootKind, "mainWindow", StringComparison.OrdinalIgnoreCase))
        {
            var timeout = mainWindowWaitTimeoutMs.HasValue
                ? TimeSpan.FromMilliseconds(mainWindowWaitTimeoutMs.Value)
                : (TimeSpan?)null;
            var mainWindow = session.Application.GetMainWindow(session.Automation, timeout);
            if (mainWindow == null)
            {
                throw HttpException.NotFound("Main window not found.");
            }
            return mainWindow;
        }

        return session.Automation.GetDesktop();
    }
}
