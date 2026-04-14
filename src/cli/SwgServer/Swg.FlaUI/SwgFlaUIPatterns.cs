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
/// 对应 AutomationController 中 #region SwgFlaUIPatterns。
/// </summary>
public static class SwgFlaUIPatterns
{
    // 01 Annotation
    /// <summary>
    /// 获取 AnnotationPattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>AnnotationPattern 属性快照。</returns>
    public static AnnotationPatternStateResult GetAnnotationPattern(string sessionId, string elementId)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.Annotation.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("AnnotationPattern is not supported.");
        }
        return new AnnotationPatternStateResult(
            pattern.AnnotationType.Value.ToString(),
            pattern.AnnotationTypeName.ValueOrDefault,
            pattern.Author.ValueOrDefault,
            pattern.DateTime.ValueOrDefault,
            AddNullableElement(session, pattern.Target.ValueOrDefault));
    }

    // 02 Dock
    /// <summary>
    /// 获取 DockPattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>DockPattern 属性快照。</returns>
    public static DockPatternStateResult GetDockPattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Dock.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("DockPattern is not supported.");
        }
        return new DockPatternStateResult(pattern.DockPosition.Value.ToString());
    }

    /// <summary>
    /// 设置 DockPattern 停靠位置。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">目标停靠位置参数。</param>
    /// <returns>操作结果。</returns>
    public static bool SetDockPattern(string sessionId, string elementId, SetDockSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Dock.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("DockPattern is not supported.");
        }
        var position = ParseEnumOrBadRequest<DockPosition>(request.Position, nameof(request.Position));
        pattern.SetDockPosition(position);
        return true;
    }

    // 03 Drag
    /// <summary>
    /// 获取 DragPattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>DragPattern 属性快照。</returns>
    public static DragPatternStateResult GetDragPattern(string sessionId, string elementId)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.Drag.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("DragPattern is not supported.");
        }
        var items = pattern.GrabbedItems.ValueOrDefault ?? Array.Empty<AutomationElement>();
        var effects = pattern.DropEffects.ValueOrDefault ?? Array.Empty<string>();
        return new DragPatternStateResult(pattern.DropEffect.ValueOrDefault, effects, pattern.IsGrabbed.Value, items.Select(session.AddElement).ToArray());
    }

    // 04 DropTarget
    /// <summary>
    /// 获取 DropTargetPattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>DropTargetPattern 属性快照。</returns>
    public static DropTargetPatternStateResult GetDropTargetPattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.DropTarget.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("DropTargetPattern is not supported.");
        }
        var effects = pattern.DropTargetEffects.ValueOrDefault ?? Array.Empty<string>();
        return new DropTargetPatternStateResult(pattern.DropTargetEffect.ValueOrDefault, effects);
    }

    /// <summary>
    /// 调用元素的 InvokePattern 动作。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>操作结果。</returns>
    public static bool Invoke(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Invoke.TryGetPattern(out var invokePattern))
        {
            throw HttpException.BadRequest("InvokePattern is not supported.");
        }

        invokePattern.Invoke();
        return true;
    }

    /// <summary>
    /// 通过 ValuePattern 设置元素值。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">要设置的值。</param>
    /// <returns>操作结果。</returns>
    public static bool SetValue(string sessionId, string elementId, SetValueSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Value.TryGetPattern(out var valuePattern))
        {
            throw HttpException.BadRequest("ValuePattern is not supported.");
        }

        valuePattern.SetValue(request.Value ?? string.Empty);
        return true;
    }

    /// <summary>
    /// 通过 ScrollItemPattern 将元素滚动到可见区域。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>操作结果。</returns>
    public static bool ScrollIntoView(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.ScrollItem.TryGetPattern(out var scrollPattern))
        {
            throw HttpException.BadRequest("ScrollItemPattern is not supported.");
        }

        scrollPattern.ScrollIntoView();
        return true;
    }

    // 08 Value
    /// <summary>
    /// 获取 ValuePattern 常用属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>返回是否只读与当前值。</returns>
    public static ValuePatternStateResult GetValuePattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Value.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("ValuePattern is not supported.");
        }

        return new ValuePatternStateResult(pattern.IsReadOnly.Value, pattern.Value.ValueOrDefault);
    }

    /// <summary>
    /// 设置 ValuePattern 的值。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">要设置的值。</param>
    /// <returns>操作结果。</returns>
    public static bool SetValuePattern(string sessionId, string elementId, SetValueSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Value.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("ValuePattern is not supported.");
        }

        pattern.SetValue(request.Value ?? string.Empty);
        return true;
    }

    // 09 RangeValue
    /// <summary>
    /// 获取 RangeValuePattern 常用属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>RangeValuePattern 属性快照。</returns>
    public static RangeValuePatternStateResult GetRangeValuePattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.RangeValue.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("RangeValuePattern is not supported.");
        }

        return new RangeValuePatternStateResult(
            pattern.IsReadOnly.Value,
            pattern.Minimum.Value,
            pattern.Maximum.Value,
            pattern.SmallChange.Value,
            pattern.LargeChange.Value,
            pattern.Value.Value);
    }

    /// <summary>
    /// 设置 RangeValuePattern 的值。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">目标数值。</param>
    /// <returns>操作结果。</returns>
    public static bool SetRangeValuePattern(string sessionId, string elementId, SetRangeValueSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.RangeValue.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("RangeValuePattern is not supported.");
        }

        pattern.SetValue(request.Value);
        return true;
    }

    // 10 Toggle
    /// <summary>
    /// 获取 TogglePattern 当前状态。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>TogglePattern 状态。</returns>
    public static TogglePatternStateResult GetTogglePattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Toggle.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("TogglePattern is not supported.");
        }

        return new TogglePatternStateResult(pattern.ToggleState.Value.ToString());
    }

    /// <summary>
    /// 执行 TogglePattern 的 Toggle 动作。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>操作结果。</returns>
    public static bool ToggleAction(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Toggle.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("TogglePattern is not supported.");
        }

        pattern.Toggle();
        return true;
    }

    /// <summary>
    /// 设置 TogglePattern 到指定状态（Off/On/Indeterminate）。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">目标 Toggle 状态。</param>
    /// <returns>操作结果。</returns>
    public static bool SetToggleState(string sessionId, string elementId, SetToggleStateSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Toggle.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("TogglePattern is not supported.");
        }

        var state = ParseEnumOrBadRequest<ToggleState>(request.State, nameof(request.State));

        // 中文注释：循环切换直到到达目标状态，最多 3 次（Off/On/Indeterminate）。
        for (var i = 0; i < 3; i++)
        {
            if (pattern.ToggleState.Value == state)
            {
                return true;
            }
            pattern.Toggle();
        }

        throw HttpException.BadRequest("Failed to set toggle state.");
    }

    // 11 ExpandCollapse
    /// <summary>
    /// 获取 ExpandCollapsePattern 当前状态。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>ExpandCollapsePattern 状态。</returns>
    public static ExpandCollapsePatternStateResult GetExpandCollapsePattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.ExpandCollapse.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("ExpandCollapsePattern is not supported.");
        }

        return new ExpandCollapsePatternStateResult(pattern.ExpandCollapseState.Value.ToString());
    }

    /// <summary>
    /// 执行 ExpandCollapsePattern 动作（expand/collapse）。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">动作参数（expand/collapse）。</param>
    /// <returns>操作结果。</returns>
    public static bool ExpandCollapseAction(string sessionId, string elementId, ExpandCollapseActionSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.ExpandCollapse.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("ExpandCollapsePattern is not supported.");
        }

        if (string.Equals(request.Action, "expand", StringComparison.OrdinalIgnoreCase))
        {
            pattern.Expand();
            return true;
        }
        if (string.Equals(request.Action, "collapse", StringComparison.OrdinalIgnoreCase))
        {
            pattern.Collapse();
            return true;
        }

        throw HttpException.BadRequest("action must be expand or collapse.");
    }

    // 12 Selection
    /// <summary>
    /// 获取 SelectionPattern 的选择信息及已选元素列表。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>SelectionPattern 属性与选中元素列表。</returns>
    public static SelectionPatternStateResult GetSelectionPattern(string sessionId, string elementId)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.Selection.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("SelectionPattern is not supported.");
        }

        var selected = pattern.Selection.ValueOrDefault ?? Array.Empty<AutomationElement>();
        var selectedIds = selected.Select(session.AddElement).ToArray();
        return new SelectionPatternStateResult(pattern.CanSelectMultiple.Value, pattern.IsSelectionRequired.Value, selectedIds);
    }

    // 13 Scroll
    /// <summary>
    /// 获取 ScrollPattern 常用属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>ScrollPattern 属性快照。</returns>
    public static ScrollPatternStateResult GetScrollPattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Scroll.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("ScrollPattern is not supported.");
        }

        return new ScrollPatternStateResult(
            pattern.HorizontallyScrollable.Value,
            pattern.HorizontalScrollPercent.Value,
            pattern.HorizontalViewSize.Value,
            pattern.VerticallyScrollable.Value,
            pattern.VerticalScrollPercent.Value,
            pattern.VerticalViewSize.Value);
    }

    /// <summary>
    /// 通过百分比设置 ScrollPattern 滚动位置。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">水平/垂直滚动百分比。</param>
    /// <returns>操作结果。</returns>
    public static bool SetScrollPercent(string sessionId, string elementId, SetScrollPercentSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Scroll.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("ScrollPattern is not supported.");
        }

        pattern.SetScrollPercent(request.HorizontalPercent, request.VerticalPercent);
        return true;
    }

    /// <summary>
    /// 通过步进动作执行 ScrollPattern 滚动。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">水平/垂直滚动步进动作。</param>
    /// <returns>操作结果。</returns>
    public static bool ScrollAction(string sessionId, string elementId, ScrollActionSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Scroll.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("ScrollPattern is not supported.");
        }

        var hAmount = ParseEnumOrBadRequest<ScrollAmount>(request.HorizontalAmount, nameof(request.HorizontalAmount));
        var vAmount = ParseEnumOrBadRequest<ScrollAmount>(request.VerticalAmount, nameof(request.VerticalAmount));

        pattern.Scroll(hAmount, vAmount);
        return true;
    }

    // 14 Window
    /// <summary>
    /// 获取 WindowPattern 常用属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>WindowPattern 属性快照。</returns>
    public static WindowPatternStateResult GetWindowPattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Window.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("WindowPattern is not supported.");
        }

        return new WindowPatternStateResult(
            pattern.CanMaximize.Value,
            pattern.CanMinimize.Value,
            pattern.IsModal.Value,
            pattern.IsTopmost.Value,
            pattern.WindowInteractionState.Value.ToString(),
            pattern.WindowVisualState.Value.ToString());
    }

    /// <summary>
    /// 设置 WindowPattern 的窗口可视状态。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">窗口可视状态参数。</param>
    /// <returns>操作结果。</returns>
    public static bool SetWindowVisualState(string sessionId, string elementId, SetWindowVisualStateSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Window.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("WindowPattern is not supported.");
        }

        var state = ParseEnumOrBadRequest<WindowVisualState>(request.State, nameof(request.State));

        pattern.SetWindowVisualState(state);
        return true;
    }

    /// <summary>
    /// 通过 WindowPattern 关闭窗口。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>操作结果。</returns>
    public static bool CloseByWindowPattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Window.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("WindowPattern is not supported.");
        }

        pattern.Close();
        return true;
    }

    /// <summary>
    /// 等待窗口进入空闲状态（WindowPattern.WaitForInputIdle）。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">等待时长参数（毫秒）。</param>
    /// <returns>是否已进入空闲状态。</returns>
    public static bool WaitInputIdle(string sessionId, string elementId, WaitForInputIdleSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Window.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("WindowPattern is not supported.");
        }

        return pattern.WaitForInputIdle(request.Milliseconds);
    }

    // 15 Selection2
    /// <summary>
    /// 获取 Selection2Pattern 常用属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>Selection2Pattern 属性快照。</returns>
    public static Selection2PatternStateResult GetSelection2Pattern(string sessionId, string elementId)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.Selection2.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("Selection2Pattern is not supported.");
        }

        return new Selection2PatternStateResult(
            pattern.ItemCount.Value,
            AddNullableElement(session, pattern.CurrentSelectedItem.ValueOrDefault),
            AddNullableElement(session, pattern.FirstSelectedItem.ValueOrDefault),
            AddNullableElement(session, pattern.LastSelectedItem.ValueOrDefault));
    }

    // 16 SelectionItem
    /// <summary>
    /// 获取 SelectionItemPattern 常用属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>SelectionItemPattern 属性快照。</returns>
    public static SelectionItemPatternStateResult GetSelectionItemPattern(string sessionId, string elementId)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.SelectionItem.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("SelectionItemPattern is not supported.");
        }

        return new SelectionItemPatternStateResult(
            pattern.IsSelected.Value,
            AddNullableElement(session, pattern.SelectionContainer.ValueOrDefault));
    }

    /// <summary>
    /// 执行 SelectionItemPattern 动作（select/add/remove）。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">动作参数。</param>
    /// <returns>操作结果。</returns>
    public static bool SelectionItemAction(string sessionId, string elementId, SelectionItemActionSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.SelectionItem.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("SelectionItemPattern is not supported.");
        }

        if (string.Equals(request.Action, "select", StringComparison.OrdinalIgnoreCase))
        {
            pattern.Select();
            return true;
        }
        if (string.Equals(request.Action, "add", StringComparison.OrdinalIgnoreCase))
        {
            pattern.AddToSelection();
            return true;
        }
        if (string.Equals(request.Action, "remove", StringComparison.OrdinalIgnoreCase))
        {
            pattern.RemoveFromSelection();
            return true;
        }
        throw HttpException.BadRequest("action must be select/add/remove.");
    }

    // 17 Transform
    /// <summary>
    /// 获取 TransformPattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>TransformPattern 属性快照。</returns>
    public static TransformPatternStateResult GetTransformPattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Transform.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("TransformPattern is not supported.");
        }
        return new TransformPatternStateResult(pattern.CanMove.Value, pattern.CanResize.Value, pattern.CanRotate.Value);
    }

    /// <summary>
    /// 执行 TransformPattern 动作（move/resize/rotate）。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">变换动作参数。</param>
    /// <returns>操作结果。</returns>
    public static bool TransformAction(string sessionId, string elementId, TransformActionSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Transform.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("TransformPattern is not supported.");
        }

        switch (request.Action?.ToLowerInvariant())
        {
            case "move":
                pattern.Move(request.X ?? 0, request.Y ?? 0);
                break;
            case "resize":
                pattern.Resize(request.Width ?? 0, request.Height ?? 0);
                break;
            case "rotate":
                pattern.Rotate(request.Degrees ?? 0);
                break;
            default:
                throw HttpException.BadRequest("action must be move/resize/rotate.");
        }
        return true;
    }

    // 18 Transform2
    /// <summary>
    /// 获取 Transform2Pattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>Transform2Pattern 属性快照。</returns>
    public static Transform2PatternStateResult GetTransform2Pattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Transform2.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("Transform2Pattern is not supported.");
        }

        return new Transform2PatternStateResult(
            pattern.CanMove.Value, pattern.CanResize.Value, pattern.CanRotate.Value,
            pattern.CanZoom.Value, pattern.ZoomLevel.Value, pattern.ZoomMinimum.Value, pattern.ZoomMaximum.Value);
    }

    /// <summary>
    /// 执行 Transform2Pattern 缩放动作（zoom/zoom-by-unit）。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">缩放动作参数。</param>
    /// <returns>操作结果。</returns>
    public static bool Transform2Action(string sessionId, string elementId, Transform2ActionSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Transform2.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("Transform2Pattern is not supported.");
        }

        if (string.Equals(request.Action, "zoom", StringComparison.OrdinalIgnoreCase))
        {
            pattern.Zoom(request.Zoom ?? 0);
            return true;
        }
        if (string.Equals(request.Action, "zoom-by-unit", StringComparison.OrdinalIgnoreCase))
        {
            var zoomUnit = ParseEnumOrBadRequest<ZoomUnit>(request.ZoomUnit, nameof(request.ZoomUnit));
            pattern.ZoomByUnit(zoomUnit);
            return true;
        }

        throw HttpException.BadRequest("action must be zoom/zoom-by-unit.");
    }

    // 19 Grid
    /// <summary>
    /// 获取 GridPattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>GridPattern 属性快照。</returns>
    public static GridPatternStateResult GetGridPattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Grid.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("GridPattern is not supported.");
        }
        return new GridPatternStateResult(pattern.RowCount.Value, pattern.ColumnCount.Value);
    }

    /// <summary>
    /// 获取 GridPattern 指定单元格元素。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">行列坐标参数。</param>
    /// <returns>单元格元素引用。</returns>
    public static ElementRefResult GetGridItem(string sessionId, string elementId, GridItemSpec request)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.Grid.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("GridPattern is not supported.");
        }
        var item = pattern.GetItem(request.Row, request.Column);
        return new ElementRefResult(session.AddElement(item));
    }

    // 20 GridItem
    /// <summary>
    /// 获取 GridItemPattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>GridItemPattern 属性快照。</returns>
    public static GridItemPatternStateResult GetGridItemPattern(string sessionId, string elementId)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.GridItem.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("GridItemPattern is not supported.");
        }
        return new GridItemPatternStateResult(
            pattern.Row.Value, pattern.RowSpan.Value, pattern.Column.Value, pattern.ColumnSpan.Value,
            AddNullableElement(session, pattern.ContainingGrid.ValueOrDefault));
    }

    // 21 Table
    /// <summary>
    /// 获取 TablePattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>TablePattern 属性快照。</returns>
    public static TablePatternStateResult GetTablePattern(string sessionId, string elementId)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.Table.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("TablePattern is not supported.");
        }

        var columnHeaders = pattern.ColumnHeaders.ValueOrDefault ?? Array.Empty<AutomationElement>();
        var rowHeaders = pattern.RowHeaders.ValueOrDefault ?? Array.Empty<AutomationElement>();
        return new TablePatternStateResult(
            pattern.RowOrColumnMajor.Value.ToString(),
            columnHeaders.Select(session.AddElement).ToArray(),
            rowHeaders.Select(session.AddElement).ToArray());
    }

    // 22 TableItem
    /// <summary>
    /// 获取 TableItemPattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>TableItemPattern 属性快照。</returns>
    public static TableItemPatternStateResult GetTableItemPattern(string sessionId, string elementId)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.TableItem.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("TableItemPattern is not supported.");
        }
        var columnHeaderItems = pattern.ColumnHeaderItems.ValueOrDefault ?? Array.Empty<AutomationElement>();
        var rowHeaderItems = pattern.RowHeaderItems.ValueOrDefault ?? Array.Empty<AutomationElement>();
        return new TableItemPatternStateResult(
            columnHeaderItems.Select(session.AddElement).ToArray(),
            rowHeaderItems.Select(session.AddElement).ToArray());
    }

    // 23 MultipleView
    /// <summary>
    /// 获取 MultipleViewPattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>MultipleViewPattern 属性快照。</returns>
    public static MultipleViewPatternStateResult GetMultipleViewPattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.MultipleView.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("MultipleViewPattern is not supported.");
        }
        var views = pattern.SupportedViews.ValueOrDefault ?? Array.Empty<int>();
        var names = views.ToDictionary(v => v.ToString(), v => pattern.GetViewName(v));
        return new MultipleViewPatternStateResult(pattern.CurrentView.Value, views, names);
    }

    /// <summary>
    /// 设置 MultipleViewPattern 当前视图。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">目标视图参数。</param>
    /// <returns>操作结果。</returns>
    public static bool SetMultipleViewPattern(string sessionId, string elementId, SetMultipleViewSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.MultipleView.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("MultipleViewPattern is not supported.");
        }
        pattern.SetCurrentView(request.View);
        return true;
    }

    // 24 ItemContainer
    /// <summary>
    /// 通过 ItemContainerPattern 查找元素。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">查找参数（起始元素、属性名、属性值）。</param>
    /// <returns>匹配元素引用。</returns>
    public static ElementRefResult FindItemByProperty(string sessionId, string elementId, ItemContainerFindSpec request)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.ItemContainer.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("ItemContainerPattern is not supported.");
        }

        AutomationElement? startAfter = null;
        if (!string.IsNullOrWhiteSpace(request.StartAfterElementId))
        {
            startAfter = ResolveElement(session, request.StartAfterElementId);
        }

        PropertyId? propertyId = null;
        if (!string.IsNullOrWhiteSpace(request.PropertyName))
        {
            var allProperties = element.Automation.PropertyLibrary.GetType()
                .GetProperties()
                .Where(p => typeof(PropertyId).IsAssignableFrom(p.PropertyType))
                .Select(p => p.GetValue(element.Automation.PropertyLibrary) as PropertyId)
                .Where(p => p != null && string.Equals(p.Name, request.PropertyName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (allProperties == null)
            {
                throw HttpException.BadRequest($"Property not found: {request.PropertyName}");
            }
            propertyId = allProperties;
        }

        var found = pattern.FindItemByProperty(startAfter, propertyId, request.Value);
        if (found == null)
        {
            throw HttpException.NotFound("Element not found.");
        }
        return new ElementRefResult(session.AddElement(found));
    }

    // 25 VirtualizedItem
    /// <summary>
    /// 调用 VirtualizedItemPattern.Realize。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>操作结果。</returns>
    public static bool RealizeVirtualizedItem(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.VirtualizedItem.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("VirtualizedItemPattern is not supported.");
        }
        pattern.Realize();
        return true;
    }

    // 26 Spreadsheet
    /// <summary>
    /// 获取 SpreadsheetPattern 指定单元格。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">按名称查找参数。</param>
    /// <returns>单元格元素引用。</returns>
    public static ElementRefResult GetSpreadsheetItemByName(string sessionId, string elementId, SpreadsheetItemByNameSpec request)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.Spreadsheet.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("SpreadsheetPattern is not supported.");
        }
        var item = pattern.GetItemByName(request.Name);
        return new ElementRefResult(session.AddElement(item));
    }

    // 27 SpreadsheetItem
    /// <summary>
    /// 获取 SpreadsheetItemPattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>SpreadsheetItemPattern 属性快照。</returns>
    public static SpreadsheetItemPatternStateResult GetSpreadsheetItemPattern(string sessionId, string elementId)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.SpreadsheetItem.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("SpreadsheetItemPattern is not supported.");
        }
        var anno = pattern.AnnotationObjects.ValueOrDefault ?? Array.Empty<AutomationElement>();
        var annoTypes = pattern.AnnotationTypes.ValueOrDefault ?? Array.Empty<AnnotationType>();
        return new SpreadsheetItemPatternStateResult(pattern.Formula.ValueOrDefault, anno.Select(session.AddElement).ToArray(), annoTypes.Select(x => x.ToString()).ToArray());
    }

    // 28 Styles
    /// <summary>
    /// 获取 StylesPattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>StylesPattern 属性快照。</returns>
    public static StylesPatternStateResult GetStylesPattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Styles.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("StylesPattern is not supported.");
        }
        return new StylesPatternStateResult(
            pattern.ExtendedProperties.ValueOrDefault,
            pattern.FillColor.Value,
            pattern.FillPatternColor.Value,
            pattern.FillPatternStyle.ValueOrDefault,
            pattern.Shape.ValueOrDefault,
            pattern.Style.Value.ToString(),
            pattern.StyleName.ValueOrDefault);
    }

    // 29 ObjectModel
    /// <summary>
    /// 获取 ObjectModelPattern 底层对象字符串。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>对象模型字符串与类型信息。</returns>
    public static ObjectModelPatternStateResult GetObjectModelPattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.ObjectModel.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("ObjectModelPattern is not supported.");
        }
        var model = pattern.GetUnderlyingObjectModel();
        return new ObjectModelPatternStateResult(model?.ToString() ?? string.Empty, model?.GetType().FullName ?? string.Empty);
    }

    // 30 LegacyIAccessible
    /// <summary>
    /// 获取 LegacyIAccessiblePattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>LegacyIAccessiblePattern 属性快照。</returns>
    public static LegacyIAccessiblePatternStateResult GetLegacyIAccessiblePattern(string sessionId, string elementId)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.LegacyIAccessible.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("LegacyIAccessiblePattern is not supported.");
        }
        var selection = pattern.Selection.ValueOrDefault ?? Array.Empty<AutomationElement>();
        return new LegacyIAccessiblePatternStateResult(
            pattern.ChildId.Value,
            pattern.DefaultAction.ValueOrDefault,
            pattern.Description.ValueOrDefault,
            pattern.Help.ValueOrDefault,
            pattern.KeyboardShortcut.ValueOrDefault,
            pattern.Name.ValueOrDefault,
            pattern.Role.Value.ToString(),
            selection.Select(session.AddElement).ToArray(),
            pattern.State.Value.ToString(),
            pattern.Value.ValueOrDefault);
    }

    /// <summary>
    /// 执行 LegacyIAccessiblePattern 动作（default/select/set-value）。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">动作参数。</param>
    /// <returns>操作结果。</returns>
    public static bool LegacyIAccessibleAction(string sessionId, string elementId, LegacyIAccessibleActionSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.LegacyIAccessible.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("LegacyIAccessiblePattern is not supported.");
        }

        if (string.Equals(request.Action, "default", StringComparison.OrdinalIgnoreCase))
        {
            pattern.DoDefaultAction();
            return true;
        }
        if (string.Equals(request.Action, "select", StringComparison.OrdinalIgnoreCase))
        {
            pattern.Select(request.FlagsSelect ?? 0);
            return true;
        }
        if (string.Equals(request.Action, "set-value", StringComparison.OrdinalIgnoreCase))
        {
            pattern.SetValue(request.Value ?? string.Empty);
            return true;
        }
        throw HttpException.BadRequest("action must be default/select/set-value.");
    }

    // 31 SynchronizedInput
    /// <summary>
    /// 执行 SynchronizedInputPattern 动作（start/cancel）。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">同步输入动作参数。</param>
    /// <returns>操作结果。</returns>
    public static bool SynchronizedInputAction(string sessionId, string elementId, SynchronizedInputActionSpec request)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.SynchronizedInput.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("SynchronizedInputPattern is not supported.");
        }
        if (string.Equals(request.Action, "cancel", StringComparison.OrdinalIgnoreCase))
        {
            pattern.Cancel();
            return true;
        }
        if (string.Equals(request.Action, "start", StringComparison.OrdinalIgnoreCase))
        {
            var inputType = ParseEnumOrBadRequest<SynchronizedInputType>(request.InputType, nameof(request.InputType));
            pattern.StartListening(inputType);
            return true;
        }
        throw HttpException.BadRequest("action must be start/cancel.");
    }

    // 32 Text
    /// <summary>
    /// 获取 TextPattern 文本概要。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>TextPattern 文本与选区摘要。</returns>
    public static TextPatternStateResult GetTextPattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Text.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("TextPattern is not supported.");
        }
        var selectedTexts = pattern.GetSelection().Select(x => x.GetText(Int32.MaxValue)).ToArray();
        var visibleTexts = pattern.GetVisibleRanges().Select(x => x.GetText(Int32.MaxValue)).ToArray();
        return new TextPatternStateResult(
            pattern.SupportedTextSelection.ToString(),
            pattern.DocumentRange.GetText(Int32.MaxValue),
            selectedTexts,
            visibleTexts);
    }

    // 33 Text2
    /// <summary>
    /// 获取 Text2Pattern 光标范围文本。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>光标范围文本与激活状态。</returns>
    public static Text2CaretRangeResult GetText2CaretRange(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.Text2.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("Text2Pattern is not supported.");
        }
        var range = pattern.GetCaretRange(out var isActive);
        return new Text2CaretRangeResult(isActive, range.GetText(Int32.MaxValue));
    }

    // 34 TextEdit
    /// <summary>
    /// 获取 TextEditPattern 组合文本。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>组合输入相关文本。</returns>
    public static TextEditPatternStateResult GetTextEditPattern(string sessionId, string elementId)
    {
        var element = ResolveElement(sessionId, elementId);
        if (!element.Patterns.TextEdit.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("TextEditPattern is not supported.");
        }
        return new TextEditPatternStateResult(
            pattern.GetActiveComposition().GetText(Int32.MaxValue),
            pattern.GetConversionTarget().GetText(Int32.MaxValue));
    }

    // 35 TextChild
    /// <summary>
    /// 获取 TextChildPattern 属性。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>文本容器引用与文本内容。</returns>
    public static TextChildPatternStateResult GetTextChildPattern(string sessionId, string elementId)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        if (!element.Patterns.TextChild.TryGetPattern(out var pattern))
        {
            throw HttpException.BadRequest("TextChildPattern is not supported.");
        }
        return new TextChildPatternStateResult(
            AddNullableElement(session, pattern.TextContainer),
            pattern.TextRange.GetText(Int32.MaxValue));
    }
}
