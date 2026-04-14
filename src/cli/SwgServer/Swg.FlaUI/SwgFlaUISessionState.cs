using System.Collections.Concurrent;
using EmbedIO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;

namespace Swg.FlaUI;

/// <summary>
/// 单次自动化会话状态：持有 Automation、Application 与元素注册表。
/// </summary>
internal sealed class SessionState : IDisposable
{
    private readonly ConcurrentDictionary<string, AutomationElement> _elements = new();
    private readonly ConcurrentDictionary<string, TypedElementState> _typed = new();

    public SessionState(string sessionId, AutomationBase automation, Application application)
    {
        SessionId = sessionId;
        Automation = automation;
        Application = application;
    }

    public string SessionId { get; }
    public AutomationBase Automation { get; }
    public Application Application { get; }

    public string AddElement(AutomationElement element)
    {
        var id = Guid.NewGuid().ToString("N");
        _elements[id] = element;
        return id;
    }

    public AutomationElement GetElement(string elementId)
    {
        if (!_elements.TryGetValue(elementId, out var element))
        {
            throw HttpException.NotFound("Element not found.");
        }
        return element;
    }

    public string AddTypedElement(string elementId, string typeName)
    {
        var id = Guid.NewGuid().ToString("N");
        _typed[id] = new TypedElementState(elementId, typeName);
        return id;
    }

    public TypedElementState GetTyped(string typedElementId)
    {
        if (!_typed.TryGetValue(typedElementId, out var typed))
        {
            throw HttpException.NotFound("Typed element not found.");
        }
        return typed;
    }

    public void Dispose()
    {
        Application.Dispose();
        Automation.Dispose();
    }
}

internal sealed record TypedElementState(string ElementId, string TypeName);
