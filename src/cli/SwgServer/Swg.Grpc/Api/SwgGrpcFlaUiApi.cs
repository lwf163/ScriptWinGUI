using Swg.FlaUI;
using Swg.Grpc.Flaui;

namespace Swg.Grpc.Api;

/// <summary>
/// FlaUI 自动化 gRPC 门面：封装 <c>Swg.FlaUI</c> 能力，与 FlaUI 相关 Proto RPC 子集一致。
/// </summary>
public static class SwgGrpcFlaUiApi
{
    public static SessionCreateResponse CreateSession(SessionCreateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var spec = new SessionCreateSpec(
            request.AutomationType,
            request.ExecutablePath,
            request.Arguments,
            request.LaunchIfNotRunning,
            request.ProcessIndex);
        SessionCreateResult r = SwgFlaUISessionApplication.CreateSession(spec);
        return new SessionCreateResponse
        {
            SessionId = r.SessionId,
            ProcessId = r.ProcessId,
            AutomationType = r.AutomationType,
        };
    }

    public static FlaUiOkResponse DeleteSession(SessionIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgFlaUISessionApplication.DeleteSession(request.SessionId);
        return new FlaUiOkResponse { Ok = true };
    }

    public static FlaUiCloseApplicationResponse CloseApplication(SessionCloseApplicationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        bool closed = SwgFlaUISessionApplication.CloseApplication(request.SessionId, new CloseApplicationSpec(request.KillIfCloseFails));
        return new FlaUiCloseApplicationResponse { Ok = true, Closed = closed };
    }

    public static FlaUiOkResponse KillApplication(SessionIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgFlaUISessionApplication.KillApplication(request.SessionId);
        return new FlaUiOkResponse { Ok = true };
    }

    public static FlaUiWaitBusyResponse WaitWhileBusy(SessionWaitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        bool result = SwgFlaUISessionApplication.WaitWhileBusy(request.SessionId, ToWaitSpec(request));
        return new FlaUiWaitBusyResponse { Ok = true, Result = result };
    }

    public static FlaUiWaitHandleResponse WaitMainHandle(SessionWaitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        bool result = SwgFlaUISessionApplication.WaitMainHandle(request.SessionId, ToWaitSpec(request));
        return new FlaUiWaitHandleResponse { Ok = true, Result = result };
    }

    public static ElementRefResponse GetMainWindow(SessionWaitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ElementRefResult r = SwgFlaUISessionApplication.GetMainWindow(request.SessionId, ToWaitSpec(request));
        return new ElementRefResponse { ElementId = r.ElementId };
    }

    public static ElementRefListResponse GetTopLevelWindows(SessionIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        IReadOnlyList<ElementRefResult> xs = SwgFlaUISessionApplication.GetTopLevelWindows(request.SessionId);
        var r = new ElementRefListResponse();
        foreach (ElementRefResult e in xs)
            r.Items.Add(new ElementRefResponse { ElementId = e.ElementId });
        return r;
    }

    public static ElementInfoResponse GetElementInfo(SessionElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ElementInfoResult info = SwgFlaUIAutomationElement.GetElementInfo(request.SessionId, request.ElementId);
        return ToElementInfo(info);
    }

    public static ElementRefResponse FindElement(SessionFindElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Find);
        ElementRefResult r = SwgFlaUIAutomationElement.FindElement(request.SessionId, ToFindElementSpec(request.Find));
        return new ElementRefResponse { ElementId = r.ElementId };
    }

    public static ElementRefListResponse FindAllElements(SessionFindElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Find);
        IReadOnlyList<ElementRefResult> xs = SwgFlaUIAutomationElement.FindAllElements(request.SessionId, ToFindElementSpec(request.Find));
        var r = new ElementRefListResponse();
        foreach (ElementRefResult e in xs)
            r.Items.Add(new ElementRefResponse { ElementId = e.ElementId });
        return r;
    }

    public static ElementRefResponse FindElementByXPath(SessionFindByXPathRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Find);
        ElementRefResult r = SwgFlaUIAutomationElement.FindElementByXPath(request.SessionId, ToFindByXPathSpec(request.Find));
        return new ElementRefResponse { ElementId = r.ElementId };
    }

    public static ElementRefListResponse FindAllElementsByXPath(SessionFindByXPathRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Find);
        IReadOnlyList<ElementRefResult> xs = SwgFlaUIAutomationElement.FindAllElementsByXPath(request.SessionId, ToFindByXPathSpec(request.Find));
        var r = new ElementRefListResponse();
        foreach (ElementRefResult e in xs)
            r.Items.Add(new ElementRefResponse { ElementId = e.ElementId });
        return r;
    }

    public static ElementRefListResponse GetChildren(SessionElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        IReadOnlyList<ElementRefResult> xs = SwgFlaUIAutomationElement.GetChildren(request.SessionId, request.ElementId);
        var r = new ElementRefListResponse();
        foreach (ElementRefResult e in xs)
            r.Items.Add(new ElementRefResponse { ElementId = e.ElementId });
        return r;
    }

    public static FlaUiOkResponse Focus(SessionElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        bool ok = SwgFlaUIAutomationElement.Focus(request.SessionId, request.ElementId);
        return new FlaUiOkResponse { Ok = ok };
    }

    public static FlaUiOkResponse FocusNative(SessionElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        bool ok = SwgFlaUIAutomationElement.FocusNative(request.SessionId, request.ElementId);
        return new FlaUiOkResponse { Ok = ok };
    }

    public static FlaUiOkResponse SetElementForeground(SessionElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        bool ok = SwgFlaUIAutomationElement.SetElementForeground(request.SessionId, request.ElementId);
        return new FlaUiOkResponse { Ok = ok };
    }

    public static FlaUiOkResponse Click(SessionElementClickRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ClickSpec? spec = ToClickSpec(request);
        bool ok = SwgFlaUIAutomationElement.Click(request.SessionId, request.ElementId, spec);
        return new FlaUiOkResponse { Ok = ok };
    }

    public static FlaUiOkResponse DoubleClick(SessionElementClickRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ClickSpec? spec = ToClickSpec(request);
        bool ok = SwgFlaUIAutomationElement.DoubleClick(request.SessionId, request.ElementId, spec);
        return new FlaUiOkResponse { Ok = ok };
    }

    public static FlaUiOkResponse RightClick(SessionElementClickRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ClickSpec? spec = ToClickSpec(request);
        bool ok = SwgFlaUIAutomationElement.RightClick(request.SessionId, request.ElementId, spec);
        return new FlaUiOkResponse { Ok = ok };
    }

    public static FlaUiOkResponse RightDoubleClick(SessionElementClickRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ClickSpec? spec = ToClickSpec(request);
        bool ok = SwgFlaUIAutomationElement.RightDoubleClick(request.SessionId, request.ElementId, spec);
        return new FlaUiOkResponse { Ok = ok };
    }

    private static WaitTimeoutSpec ToWaitSpec(SessionWaitRequest r)
    {
        if (r.Timeout is null)
            return new WaitTimeoutSpec(null);
        return new WaitTimeoutSpec(r.Timeout.HasTimeoutMs ? r.Timeout.TimeoutMs : null);
    }

    private static FindElementSpec ToFindElementSpec(FindElementPayload p) =>
        new(
            p.RootKind,
            p.RootElementId,
            p.Scope,
            p.AutomationId,
            p.Name,
            p.ClassName,
            p.ControlType,
            p.Xpath,
            p.HasMainWindowWaitTimeoutMs ? p.MainWindowWaitTimeoutMs : null);

    private static FindByXPathSpec ToFindByXPathSpec(FindByXPathPayload p) =>
        new(
            p.RootKind,
            p.RootElementId,
            p.Xpath,
            p.HasMainWindowWaitTimeoutMs ? p.MainWindowWaitTimeoutMs : null);

    private static ElementInfoResponse ToElementInfo(ElementInfoResult r) =>
        new()
        {
            ElementId = r.ElementId,
            Name = r.Name,
            AutomationId = r.AutomationId,
            ClassName = r.ClassName,
            ControlType = r.ControlType,
            FrameworkType = r.FrameworkType,
            IsEnabled = r.IsEnabled,
            IsOffscreen = r.IsOffscreen,
            IsAvailable = r.IsAvailable,
            Bounds = new RectDto
            {
                X = r.Bounds.X,
                Y = r.Bounds.Y,
                Width = r.Bounds.Width,
                Height = r.Bounds.Height,
            },
        };

    private static ClickSpec? ToClickSpec(SessionElementClickRequest request)
    {
        if (!request.HasClick || request.Click is null)
            return null;
        bool move = request.Click.HasMoveMouse && request.Click.MoveMouse;
        return new ClickSpec(move);
    }
}
