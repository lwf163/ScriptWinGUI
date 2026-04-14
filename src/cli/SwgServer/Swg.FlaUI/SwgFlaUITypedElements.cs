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
/// 对应 AutomationController 中 #region SwgFlaUITypedElements。
/// </summary>
public static class SwgFlaUITypedElements
{
    /// <summary>
    /// 获取 typed 元素的类型元数据。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="typedElementId">typed 元素 ID。</param>
    /// <returns>typed 元素元数据。</returns>
    public static TypedElementRefResult GetTypedElement(string sessionId, string typedElementId)
    {
        var typed = ResolveSession(sessionId).GetTyped(typedElementId);
        return new TypedElementRefResult(typedElementId, typed.TypeName);
    }

    /// <summary>
    /// 获取 TextBox 文本内容。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="typedElementId">typed 元素 ID（需为 TextBox）。</param>
    /// <returns>TextBox 文本值。</returns>
    public static string GetTextBoxText(string sessionId, string typedElementId)
    {
        var tb = ResolveTyped<TextBox>(sessionId, typedElementId, "TextBox", e => e.AsTextBox());
        return tb.Text;
    }

    /// <summary>
    /// 设置 TextBox 文本内容。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="typedElementId">typed 元素 ID（需为 TextBox）。</param>
    /// <param name="request">文本值参数。</param>
    /// <returns>操作结果。</returns>
    public static bool SetTextBoxText(string sessionId, string typedElementId, SetValueSpec request)
    {
        var tb = ResolveTyped<TextBox>(sessionId, typedElementId, "TextBox", e => e.AsTextBox());
        tb.Text = request.Value ?? string.Empty;
        return true;
    }

    /// <summary>
    /// 按索引或文本选择 ComboBox 项。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="typedElementId">typed 元素 ID（需为 ComboBox）。</param>
    /// <param name="request">选择参数，支持 index 或 text。</param>
    /// <returns>操作结果。</returns>
    public static bool SelectComboBox(string sessionId, string typedElementId, ComboBoxSelectSpec request)
    {
        var cb = ResolveTyped<ComboBox>(sessionId, typedElementId, "ComboBox", e => e.AsComboBox());
        if (request.Index is int index)
        {
            cb.Select(index);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(request.Text))
        {
            var item = cb.Select(request.Text);
            if (item == null)
            {
                throw HttpException.NotFound("ComboBox item not found.");
            }
            return true;
        }

        throw HttpException.BadRequest("index or text is required.");
    }

    /// <summary>
    /// 获取 CheckBox 选中状态。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="typedElementId">typed 元素 ID（需为 CheckBox）。</param>
    /// <returns>CheckBox 当前选中状态。</returns>
    public static bool? GetCheckBoxState(string sessionId, string typedElementId)
    {
        var cb = ResolveTyped<CheckBox>(sessionId, typedElementId, "CheckBox", e => e.AsCheckBox());
        return cb.IsChecked;
    }

    /// <summary>
    /// 设置 CheckBox 选中状态。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="typedElementId">typed 元素 ID（需为 CheckBox）。</param>
    /// <param name="request">目标选中状态参数。</param>
    /// <returns>操作结果。</returns>
    public static bool SetCheckBoxState(string sessionId, string typedElementId, SetCheckBoxStateSpec request)
    {
        var cb = ResolveTyped<CheckBox>(sessionId, typedElementId, "CheckBox", e => e.AsCheckBox());
        cb.IsChecked = request.IsChecked;
        return true;
    }

    /// <summary>
    /// 关闭 typed Window。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="typedElementId">typed 元素 ID（需为 Window）。</param>
    /// <returns>操作结果。</returns>
    public static bool CloseWindow(string sessionId, string typedElementId)
    {
        var window = ResolveTyped<Window>(sessionId, typedElementId, "Window", e => e.AsWindow());
        window.Close();
        return true;
    }

    /// <summary>
    /// 将 typed Window 置于前台。
    /// </summary>
    /// <param name="sessionId">会话 ID。</param>
    /// <param name="typedElementId">typed 元素 ID（需为 Window）。</param>
    /// <returns>操作结果。</returns>
    public static bool SetWindowForeground(string sessionId, string typedElementId)
    {
        var window = ResolveTyped<Window>(sessionId, typedElementId, "Window", e => e.AsWindow());
        window.SetForeground();
        return true;
    }

    private static T ResolveTyped<T>(string sessionId, string typedElementId, string expectedType, Func<AutomationElement, T?> converter) where T : AutomationElement
    {
        var session = ResolveSession(sessionId);
        var typed = session.GetTyped(typedElementId);
        if (!typed.TypeName.Equals(expectedType, StringComparison.OrdinalIgnoreCase))
        {
            throw HttpException.BadRequest($"Typed element is {typed.TypeName}, expected {expectedType}.");
        }

        var raw = ResolveElement(session, typed.ElementId);
        var converted = converter(raw);
        if (converted == null)
        {
            throw HttpException.BadRequest($"Failed to convert element to {expectedType}.");
        }

        return converted;
    }
}
