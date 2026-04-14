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
/// 对应 AutomationController 中 #region SwgFlaUIElementMeta。
/// </summary>
public static class SwgFlaUIElementMeta
{
    private static readonly string[] SupportedTypedNames =
    {
        "Button", "Calendar", "CheckBox", "ComboBox", "DataGridView", "DateTimePicker",
        "Grid", "GridRow", "GridCell", "GridHeader", "GridHeaderItem", "Label",
        "HorizontalScrollBar", "VerticalScrollBar", "ListBox", "ListBoxItem", "Menu", "MenuItem", "ProgressBar",
        "RadioButton", "Slider", "Spinner", "Tab", "TabItem", "TextBox", "Thumb",
        "TitleBar", "ToggleButton", "Tree", "TreeItem", "Window"
    };

    /// <summary>
    /// 获取当前元素支持的 Pattern 名称列表。
    /// </summary>
    public static IReadOnlyList<string> GetSupportedPatterns(string sessionId, string elementId)
    {
        return ResolveElement(sessionId, elementId).GetSupportedPatterns().Select(x => x.Name).ToArray();
    }

    /// <summary>
    /// 截取当前元素截图并返回 Base64 PNG。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>元素截图（Base64 PNG）。</returns>
    public static ScreenshotResult CaptureElement(string sessionId, string elementId)
    {
        using var bitmap = ResolveElement(sessionId, elementId).Capture();
        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return new ScreenshotResult(Convert.ToBase64String(ms.ToArray()));
    }

    /// <summary>
    /// 获取元素可转换的控件类型列表（AsXxx）。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <returns>可转换类型名称列表。</returns>
    public static IReadOnlyList<string> GetSupportedTypes(string sessionId, string elementId)
    {
        _ = ResolveElement(sessionId, elementId);
        return SupportedTypedNames;
    }

    /// <summary>
    /// 将元素声明为指定控件类型并返回 typedElementId。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="elementId">元素 ID。</param>
    /// <param name="request">目标控件类型名称。</param>
    /// <returns>typed 元素引用。</returns>
    public static TypedElementRefResult AsType(string sessionId, string elementId, AsTypeSpec request)
    {
        var session = ResolveSession(sessionId);
        var element = ResolveElement(session, elementId);
        var name = request.Type?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw HttpException.BadRequest("type is required.");
        }

        EnsureAsType(element, name);
        var typedId = session.AddTypedElement(elementId, name);
        return new TypedElementRefResult(typedId, name);
    }

    private static void EnsureAsType(AutomationElement element, string typeName)
    {
        _ = typeName.ToLowerInvariant() switch
        {
            "button" => (AutomationElement)element.AsButton(),
            "calendar" => (AutomationElement)element.AsCalendar(),
            "checkbox" => (AutomationElement)element.AsCheckBox(),
            "combobox" => (AutomationElement)element.AsComboBox(),
            "datagridview" => (AutomationElement)element.AsDataGridView(),
            "datetimepicker" => (AutomationElement)element.AsDateTimePicker(),
            "grid" => (AutomationElement)element.AsGrid(),
            "gridrow" => (AutomationElement)element.AsGridRow(),
            "gridcell" => (AutomationElement)element.AsGridCell(),
            "gridheader" => (AutomationElement)element.AsGridHeader(),
            "gridheaderitem" => (AutomationElement)element.AsGridHeaderItem(),
            "label" => (AutomationElement)element.AsLabel(),
            "horizontalscrollbar" => (AutomationElement)element.AsHorizontalScrollBar(),
            "listbox" => (AutomationElement)element.AsListBox(),
            "listboxitem" => (AutomationElement)element.AsListBoxItem(),
            "menu" => (AutomationElement)element.AsMenu(),
            "menuitem" => (AutomationElement)element.AsMenuItem(),
            "progressbar" => (AutomationElement)element.AsProgressBar(),
            "radiobutton" => (AutomationElement)element.AsRadioButton(),
            "slider" => (AutomationElement)element.AsSlider(),
            "spinner" => (AutomationElement)element.AsSpinner(),
            "tab" => (AutomationElement)element.AsTab(),
            "tabitem" => (AutomationElement)element.AsTabItem(),
            "textbox" => (AutomationElement)element.AsTextBox(),
            "thumb" => (AutomationElement)element.AsThumb(),
            "titlebar" => (AutomationElement)element.AsTitleBar(),
            "togglebutton" => (AutomationElement)element.AsToggleButton(),
            "tree" => (AutomationElement)element.AsTree(),
            "treeitem" => (AutomationElement)element.AsTreeItem(),
            "verticalscrollbar" => (AutomationElement)element.AsVerticalScrollBar(),
            "window" => (AutomationElement)element.AsWindow(),
            _ => throw HttpException.BadRequest($"Unsupported type: {typeName}")
        };
    }
}
