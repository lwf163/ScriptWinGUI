using Swg.FlaUI;
using Swg.Grpc.Flaui;

namespace Swg.Grpc.Api;

/// <summary>
/// FlaUI UI 自动化 gRPC 门面：封装 <c>Swg.FlaUI</c> 能力，与 FlaUI 相关 Proto RPC 子集一致。
/// <para>
/// 提供基于 FlaUI 框架的 Windows UI 自动化能力，包括会话管理、元素查找/遍历、
/// 鼠标点击模拟、焦点控制等功能。所有操作均在会话上下文中执行。
/// </para>
/// <para>对应 Proto 服务：<c>swg.flaui.AutomationService</c></para>
/// <para>
/// 异常映射约定（经由 <see cref="GrpcRouteRunner"/> 统一处理）：
/// <list type="bullet">
///   <item><description><see cref="ArgumentException"/> → <c>StatusCode.InvalidArgument</c></description></item>
///   <item><description><see cref="InvalidOperationException"/> → <c>StatusCode.Unavailable</c></description></item>
///   <item><description>其他异常 → <c>StatusCode.Internal</c></description></item>
/// </list>
/// </para>
/// </summary>
public static class SwgGrpcFlaUiApi
{
    /// <summary>
    /// 创建 UI 自动化会话，关联到目标应用程序进程。
    /// <para>
    /// 若目标进程未运行且 <c>LaunchIfNotRunning</c> 为 true，将启动指定可执行文件。
    /// </para>
    /// </summary>
    /// <param name="request">
    /// 创建会话请求参数：
    /// <list type="bullet">
    ///   <item><description><c>AutomationType</c>（string，可选）：自动化框架类型（如 <c>UIA3</c>、<c>UIA2</c>）</description></item>
    ///   <item><description><c>ExecutablePath</c>（string，可选）：目标可执行文件路径</description></item>
    ///   <item><description><c>Arguments</c>（string，可选）：启动参数</description></item>
    ///   <item><description><c>LaunchIfNotRunning</c>（bool）：进程未运行时是否自动启动</description></item>
    ///   <item><description><c>ProcessIndex</c>（int32，可选）：同名多实例时的进程索引（0 起始）</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="SessionCreateResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>SessionId</c>（string）：会话唯一标识，后续所有操作需使用此 ID</description></item>
    ///   <item><description><c>ProcessId</c>（int32）：关联的进程 ID</description></item>
    ///   <item><description><c>AutomationType</c>（string）：实际使用的自动化框架类型</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
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

    /// <summary>
    /// 删除指定会话，释放自动化资源（不关闭应用程序）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SessionId</c>（string，必填）：由 <see cref="CreateSession"/> 返回的会话 ID</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="FlaUiOkResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Ok</c>（bool）：操作是否成功</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static FlaUiOkResponse DeleteSession(SessionIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgFlaUISessionApplication.DeleteSession(request.SessionId);
        return new FlaUiOkResponse { Ok = true };
    }

    /// <summary>
    /// 优雅关闭会话关联的应用程序。
    /// <para>
    /// 尝试发送关闭信号，若失败且 <c>KillIfCloseFails</c> 为 true，则强制终止进程。
    /// </para>
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SessionId</c>（string，必填）：会话 ID</description></item>
    ///   <item><description><c>KillIfCloseFails</c>（bool）：优雅关闭失败时是否强制终止</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="FlaUiCloseApplicationResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Ok</c>（bool）：操作是否成功执行</description></item>
    ///   <item><description><c>Closed</c>（bool）：应用程序是否已被关闭（优雅关闭失败且未启用 Kill 时为 false）</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static FlaUiCloseApplicationResponse CloseApplication(SessionCloseApplicationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        bool closed = SwgFlaUISessionApplication.CloseApplication(request.SessionId, new CloseApplicationSpec(request.KillIfCloseFails));
        return new FlaUiCloseApplicationResponse { Ok = true, Closed = closed };
    }

    /// <summary>
    /// 强制终止会话关联的应用程序进程。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SessionId</c>（string，必填）：会话 ID</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="FlaUiOkResponse"/>，<c>Ok</c> 为 true 表示操作成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static FlaUiOkResponse KillApplication(SessionIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        SwgFlaUISessionApplication.KillApplication(request.SessionId);
        return new FlaUiOkResponse { Ok = true };
    }

    /// <summary>
    /// 等待会话关联的应用程序进入空闲状态（主窗口无待处理消息）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SessionId</c>（string，必填）：会话 ID</description></item>
    ///   <item><description><c>Timeout</c>（<see cref="WaitTimeoutPayload"/>，可选）：超时设置，含 <c>HasTimeoutMs</c>/<c>TimeoutMs</c></description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="FlaUiWaitBusyResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Ok</c>（bool）：操作是否成功执行</description></item>
    ///   <item><description><c>Result</c>（bool）：是否在超时前进入空闲状态</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static FlaUiWaitBusyResponse WaitWhileBusy(SessionWaitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        bool result = SwgFlaUISessionApplication.WaitWhileBusy(request.SessionId, ToWaitSpec(request));
        return new FlaUiWaitBusyResponse { Ok = true, Result = result };
    }

    /// <summary>
    /// 等待会话关联的应用程序主窗口句柄可用。
    /// </summary>
    /// <param name="request">
    /// 请求参数：同 <see cref="WaitWhileBusy"/>。
    /// </param>
    /// <returns>
    /// <see cref="FlaUiWaitHandleResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Ok</c>（bool）：操作是否成功执行</description></item>
    ///   <item><description><c>Result</c>（bool）：是否在超时前获取到主窗口句柄</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static FlaUiWaitHandleResponse WaitMainHandle(SessionWaitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        bool result = SwgFlaUISessionApplication.WaitMainHandle(request.SessionId, ToWaitSpec(request));
        return new FlaUiWaitHandleResponse { Ok = true, Result = result };
    }

    /// <summary>
    /// 获取会话关联应用程序的主窗口元素引用。
    /// </summary>
    /// <param name="request">
    /// 请求参数：同 <see cref="WaitWhileBusy"/>。
    /// </param>
    /// <returns>
    /// <see cref="ElementRefResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>ElementId</c>（string）：主窗口元素的唯一标识</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static ElementRefResponse GetMainWindow(SessionWaitRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ElementRefResult r = SwgFlaUISessionApplication.GetMainWindow(request.SessionId, ToWaitSpec(request));
        return new ElementRefResponse { ElementId = r.ElementId };
    }

    /// <summary>
    /// 获取会话关联应用程序的所有顶级窗口元素引用。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SessionId</c>（string，必填）：会话 ID</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ElementRefListResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Items</c>（repeated <see cref="ElementRefResponse"/>）：顶级窗口元素引用列表</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static ElementRefListResponse GetTopLevelWindows(SessionIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        IReadOnlyList<ElementRefResult> xs = SwgFlaUISessionApplication.GetTopLevelWindows(request.SessionId);
        var r = new ElementRefListResponse();
        foreach (ElementRefResult e in xs)
            r.Items.Add(new ElementRefResponse { ElementId = e.ElementId });
        return r;
    }

    /// <summary>
    /// 获取指定 UI 元素的详细信息（名称、类型、位置、状态等）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SessionId</c>（string，必填）：会话 ID</description></item>
    ///   <item><description><c>ElementId</c>（string，必填）：目标元素 ID</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ElementInfoResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>ElementId</c>（string）：元素唯一标识</description></item>
    ///   <item><description><c>Name</c>（string）：元素名称</description></item>
    ///   <item><description><c>AutomationId</c>（string）：自动化 ID</description></item>
    ///   <item><description><c>ClassName</c>（string）：类名</description></item>
    ///   <item><description><c>ControlType</c>（string）：控件类型（如 Button、Edit 等）</description></item>
    ///   <item><description><c>FrameworkType</c>（string）：UI 框架类型</description></item>
    ///   <item><description><c>IsEnabled</c>（bool）：是否启用</description></item>
    ///   <item><description><c>IsOffscreen</c>（bool）：是否在屏幕外</description></item>
    ///   <item><description><c>IsAvailable</c>（bool）：是否可用</description></item>
    ///   <item><description><c>Bounds</c>（<see cref="RectDto"/>）：元素边界矩形（X, Y, Width, Height）</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static ElementInfoResponse GetElementInfo(SessionElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ElementInfoResult info = SwgFlaUIAutomationElement.GetElementInfo(request.SessionId, request.ElementId);
        return ToElementInfo(info);
    }

    /// <summary>
    /// 根据条件查找第一个匹配的 UI 元素。
    /// <para>
    /// 支持按 AutomationId、Name、ClassName、ControlType 等条件组合查找，
    /// 也可使用 XPath 表达式进行高级查找。
    /// </para>
    /// </summary>
    /// <param name="request">
    /// 查找请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SessionId</c>（string，必填）：会话 ID</description></item>
    ///   <item><description><c>Find</c>（<see cref="FindElementPayload"/>，必填）：查找条件，含 <c>RootKind</c>/<c>RootElementId</c>/<c>Scope</c>/<c>AutomationId</c>/<c>Name</c>/<c>ClassName</c>/<c>ControlType</c>/<c>Xpath</c> 等</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="ElementRefResponse"/>，含匹配元素的 <c>ElementId</c></returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 或 <c>Find</c> 为 null</exception>
    public static ElementRefResponse FindElement(SessionFindElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Find);
        ElementRefResult r = SwgFlaUIAutomationElement.FindElement(request.SessionId, ToFindElementSpec(request.Find));
        return new ElementRefResponse { ElementId = r.ElementId };
    }

    /// <summary>
    /// 根据条件查找所有匹配的 UI 元素。
    /// </summary>
    /// <param name="request">查找请求参数：同 <see cref="FindElement"/>。</param>
    /// <returns>
    /// <see cref="ElementRefListResponse"/>，含所有匹配元素的 <c>ElementId</c> 列表。
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 或 <c>Find</c> 为 null</exception>
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

    /// <summary>
    /// 使用 XPath 表达式查找第一个匹配的 UI 元素。
    /// </summary>
    /// <param name="request">
    /// 查找请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SessionId</c>（string，必填）：会话 ID</description></item>
    ///   <item><description><c>Find</c>（<see cref="FindByXPathPayload"/>，必填）：XPath 查找条件，含 <c>RootKind</c>/<c>RootElementId</c>/<c>Xpath</c></description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="ElementRefResponse"/>，含匹配元素的 <c>ElementId</c></returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 或 <c>Find</c> 为 null</exception>
    public static ElementRefResponse FindElementByXPath(SessionFindByXPathRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Find);
        ElementRefResult r = SwgFlaUIAutomationElement.FindElementByXPath(request.SessionId, ToFindByXPathSpec(request.Find));
        return new ElementRefResponse { ElementId = r.ElementId };
    }

    /// <summary>
    /// 使用 XPath 表达式查找所有匹配的 UI 元素。
    /// </summary>
    /// <param name="request">查找请求参数：同 <see cref="FindElementByXPath"/>。</param>
    /// <returns><see cref="ElementRefListResponse"/>，含所有匹配元素的 <c>ElementId</c> 列表</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 或 <c>Find</c> 为 null</exception>
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

    /// <summary>
    /// 获取指定 UI 元素的所有直接子元素。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SessionId</c>（string，必填）：会话 ID</description></item>
    ///   <item><description><c>ElementId</c>（string，必填）：父元素 ID</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="ElementRefListResponse"/>，含子元素的 <c>ElementId</c> 列表</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static ElementRefListResponse GetChildren(SessionElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        IReadOnlyList<ElementRefResult> xs = SwgFlaUIAutomationElement.GetChildren(request.SessionId, request.ElementId);
        var r = new ElementRefListResponse();
        foreach (ElementRefResult e in xs)
            r.Items.Add(new ElementRefResponse { ElementId = e.ElementId });
        return r;
    }

    /// <summary>
    /// 将输入焦点设置到指定 UI 元素。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SessionId</c>（string，必填）：会话 ID</description></item>
    ///   <item><description><c>ElementId</c>（string，必填）：目标元素 ID</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="FlaUiOkResponse"/>，<c>Ok</c> 表示是否成功聚焦</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static FlaUiOkResponse Focus(SessionElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        bool ok = SwgFlaUIAutomationElement.Focus(request.SessionId, request.ElementId);
        return new FlaUiOkResponse { Ok = ok };
    }

    /// <summary>
    /// 使用 Win32 SetFocus 设置焦点到指定 UI 元素（绕过 FlaUI 抽象层）。
    /// </summary>
    /// <param name="request">请求参数：同 <see cref="Focus"/>。</param>
    /// <returns><see cref="FlaUiOkResponse"/>，<c>Ok</c> 表示是否成功聚焦</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static FlaUiOkResponse FocusNative(SessionElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        bool ok = SwgFlaUIAutomationElement.FocusNative(request.SessionId, request.ElementId);
        return new FlaUiOkResponse { Ok = ok };
    }

    /// <summary>
    /// 将指定 UI 元素所在窗口设为前台窗口。
    /// </summary>
    /// <param name="request">请求参数：同 <see cref="Focus"/>。</param>
    /// <returns><see cref="FlaUiOkResponse"/>，<c>Ok</c> 表示是否成功设置</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static FlaUiOkResponse SetElementForeground(SessionElementRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        bool ok = SwgFlaUIAutomationElement.SetElementForeground(request.SessionId, request.ElementId);
        return new FlaUiOkResponse { Ok = ok };
    }

    /// <summary>
    /// 对指定 UI 元素执行鼠标左键单击。
    /// </summary>
    /// <param name="request">
    /// 点击请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SessionId</c>（string，必填）：会话 ID</description></item>
    ///   <item><description><c>ElementId</c>（string，必填）：目标元素 ID</description></item>
    ///   <item><description><c>HasClick</c>（bool）：是否包含点击选项</description></item>
    ///   <item><description><c>Click</c>（<see cref="ClickPayload"/>）：点击选项，含 <c>HasMoveMouse</c>/<c>MoveMouse</c>（是否先移动鼠标到元素位置）</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="FlaUiOkResponse"/>，<c>Ok</c> 表示操作是否成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static FlaUiOkResponse Click(SessionElementClickRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ClickSpec? spec = ToClickSpec(request);
        bool ok = SwgFlaUIAutomationElement.Click(request.SessionId, request.ElementId, spec);
        return new FlaUiOkResponse { Ok = ok };
    }

    /// <summary>
    /// 对指定 UI 元素执行鼠标左键双击。
    /// </summary>
    /// <param name="request">点击请求参数：同 <see cref="Click"/>。</param>
    /// <returns><see cref="FlaUiOkResponse"/>，<c>Ok</c> 表示操作是否成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static FlaUiOkResponse DoubleClick(SessionElementClickRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ClickSpec? spec = ToClickSpec(request);
        bool ok = SwgFlaUIAutomationElement.DoubleClick(request.SessionId, request.ElementId, spec);
        return new FlaUiOkResponse { Ok = ok };
    }

    /// <summary>
    /// 对指定 UI 元素执行鼠标右键单击。
    /// </summary>
    /// <param name="request">点击请求参数：同 <see cref="Click"/>。</param>
    /// <returns><see cref="FlaUiOkResponse"/>，<c>Ok</c> 表示操作是否成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    public static FlaUiOkResponse RightClick(SessionElementClickRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ClickSpec? spec = ToClickSpec(request);
        bool ok = SwgFlaUIAutomationElement.RightClick(request.SessionId, request.ElementId, spec);
        return new FlaUiOkResponse { Ok = ok };
    }

    /// <summary>
    /// 对指定 UI 元素执行鼠标右键双击。
    /// </summary>
    /// <param name="request">点击请求参数：同 <see cref="Click"/>。</param>
    /// <returns><see cref="FlaUiOkResponse"/>，<c>Ok</c> 表示操作是否成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
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
