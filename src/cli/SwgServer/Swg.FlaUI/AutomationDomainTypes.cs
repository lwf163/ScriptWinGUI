namespace Swg.FlaUI;

/// <summary>
/// 自动化领域用值类型（Spec/Result 与 gRPC Proto 等对外契约解耦）。
/// </summary>

/// <summary>创建会话请求参数。</summary>
/// <param name="AutomationType">自动化类型。有效值：UIA2、UIA3。</param>
/// <param name="ExecutablePath">目标进程可执行文件路径。</param>
/// <param name="Arguments">启动参数。</param>
/// <param name="LaunchIfNotRunning">未运行时是否允许自动启动。</param>
/// <param name="ProcessIndex">匹配多个进程时的索引。</param>
public sealed record SessionCreateSpec(
    string? AutomationType,
    string? ExecutablePath,
    string? Arguments,
    bool LaunchIfNotRunning,
    int ProcessIndex);
/// <summary>创建会话响应。</summary>
/// <param name="SessionId">会话标识。</param>
/// <param name="ProcessId">关联进程 ID。</param>
/// <param name="AutomationType">实际使用的自动化类型。</param>
public sealed record SessionCreateResult(string SessionId, int ProcessId, string AutomationType);
/// <summary>关闭应用请求参数。</summary>
/// <param name="KillIfCloseFails">关闭失败时是否强制终止进程。</param>
public sealed record CloseApplicationSpec(bool KillIfCloseFails = true);
/// <summary>通用等待超时参数（毫秒）。</summary>
/// <param name="TimeoutMs">超时时间（毫秒），null 表示使用默认行为。</param>
public sealed record WaitTimeoutSpec(int? TimeoutMs);

/// <summary>条件查找元素请求参数。</summary>
/// <param name="RootKind">根节点类型，如 mainWindow。</param>
/// <param name="RootElementId">根元素 ID，优先于 RootKind。</param>
/// <param name="Scope">查找范围：children/descendants/subtree。</param>
/// <param name="AutomationId">按 AutomationId 过滤。</param>
/// <param name="Name">按 Name 过滤。</param>
/// <param name="ClassName">按 ClassName 过滤。</param>
/// <param name="ControlType">按 ControlType 过滤。有效值：Unknown、AppBar、Button、Calendar、CheckBox、ComboBox、Custom、DataGrid、DataItem、Document、Edit、Group、Header、HeaderItem、Hyperlink、Image、List、ListItem、MenuBar、Menu、MenuItem、Pane、ProgressBar、RadioButton、ScrollBar、SemanticZoom、Separator、Slider、Spinner、SplitButton、StatusBar、Tab、TabItem、Table、Text、Thumb、TitleBar、ToolBar、ToolTip、Tree、TreeItem、Window。</param>
/// <param name="XPath">XPath 条件（此请求不直接使用）。</param>
/// <param name="MainWindowWaitTimeoutMs">主窗口等待超时（毫秒）。</param>
public sealed record FindElementSpec(
    string? RootKind,
    string? RootElementId,
    string? Scope,
    string? AutomationId,
    string? Name,
    string? ClassName,
    string? ControlType,
    string? XPath,
    int? MainWindowWaitTimeoutMs);
/// <summary>XPath 查找元素请求参数。</summary>
/// <param name="RootKind">根节点类型，如 mainWindow。</param>
/// <param name="RootElementId">根元素 ID，优先于 RootKind。</param>
/// <param name="XPath">XPath 表达式。</param>
/// <param name="MainWindowWaitTimeoutMs">主窗口等待超时（毫秒）。</param>
public sealed record FindByXPathSpec(string? RootKind, string? RootElementId, string? XPath, int? MainWindowWaitTimeoutMs);

/// <summary>元素引用响应。</summary>
/// <param name="ElementId">元素标识。</param>
public sealed record ElementRefResult(string ElementId);
/// <summary>类型化元素引用响应。</summary>
/// <param name="TypedElementId">类型化元素标识。</param>
/// <param name="Type">声明的控件类型。</param>
public sealed record TypedElementRefResult(string TypedElementId, string Type);
/// <summary>点击参数。</summary>
/// <param name="MoveMouse">是否先移动鼠标到目标再点击。</param>
public sealed record ClickSpec(bool MoveMouse);
/// <summary>字符串值设置参数。</summary>
/// <param name="Value">要设置的字符串值。</param>
public sealed record SetValueSpec(string? Value);
/// <summary>复选框状态设置参数。</summary>
/// <param name="IsChecked">目标状态，true/false/null（不确定）。</param>
public sealed record SetCheckBoxStateSpec(bool? IsChecked);
/// <summary>下拉框选择参数。</summary>
/// <param name="Index">按索引选择。</param>
/// <param name="Text">按文本选择。</param>
public sealed record ComboBoxSelectSpec(int? Index, string? Text);
/// <summary>截图响应（Base64 PNG）。</summary>
/// <param name="Base64Png">Base64 编码的 PNG 图像。</param>
public sealed record ScreenshotResult(string Base64Png);
/// <summary>坐标点响应。</summary>
/// <param name="X">X 坐标。</param>
/// <param name="Y">Y 坐标。</param>
public sealed record PointResult(int X, int Y);
/// <summary>尝试获取坐标点响应。</summary>
/// <param name="Success">是否获取成功。</param>
/// <param name="X">X 坐标。</param>
/// <param name="Y">Y 坐标。</param>
public sealed record TryPointResult(bool Success, int X, int Y);
/// <summary>ValuePattern 状态响应。</summary>
/// <param name="IsReadOnly">是否只读。</param>
/// <param name="Value">当前值。</param>
public sealed record ValuePatternStateResult(bool IsReadOnly, string? Value);
/// <summary>RangeValue 设置参数。</summary>
/// <param name="Value">目标数值。</param>
public sealed record SetRangeValueSpec(double Value);
/// <summary>RangeValuePattern 状态响应。</summary>
/// <param name="IsReadOnly">是否只读。</param>
/// <param name="Minimum">最小值。</param>
/// <param name="Maximum">最大值。</param>
/// <param name="SmallChange">小步进。</param>
/// <param name="LargeChange">大步进。</param>
/// <param name="Value">当前值。</param>
public sealed record RangeValuePatternStateResult(bool IsReadOnly, double Minimum, double Maximum, double SmallChange, double LargeChange, double Value);
/// <summary>TogglePattern 状态响应。</summary>
/// <param name="ToggleState">当前切换状态。</param>
public sealed record TogglePatternStateResult(string ToggleState);
/// <summary>Toggle 状态设置参数。</summary>
/// <param name="State">目标切换状态。有效值：Off、On、Indeterminate。</param>
public sealed record SetToggleStateSpec(string State);
/// <summary>ExpandCollapsePattern 状态响应。</summary>
/// <param name="ExpandCollapseState">展开/折叠状态。</param>
public sealed record ExpandCollapsePatternStateResult(string ExpandCollapseState);
/// <summary>Expand/Collapse 动作参数。</summary>
/// <param name="Action">动作：expand/collapse。</param>
public sealed record ExpandCollapseActionSpec(string Action);
/// <summary>SelectionPattern 状态响应。</summary>
/// <param name="CanSelectMultiple">是否允许多选。</param>
/// <param name="IsSelectionRequired">是否必须有选中项。</param>
/// <param name="SelectedElementIds">已选元素 ID 列表。</param>
public sealed record SelectionPatternStateResult(bool CanSelectMultiple, bool IsSelectionRequired, IReadOnlyList<string> SelectedElementIds);
/// <summary>ScrollPattern 状态响应。</summary>
/// <param name="HorizontallyScrollable">是否支持水平滚动。</param>
/// <param name="HorizontalScrollPercent">水平滚动百分比。</param>
/// <param name="HorizontalViewSize">水平视口大小百分比。</param>
/// <param name="VerticallyScrollable">是否支持垂直滚动。</param>
/// <param name="VerticalScrollPercent">垂直滚动百分比。</param>
/// <param name="VerticalViewSize">垂直视口大小百分比。</param>
public sealed record ScrollPatternStateResult(
    bool HorizontallyScrollable,
    double HorizontalScrollPercent,
    double HorizontalViewSize,
    bool VerticallyScrollable,
    double VerticalScrollPercent,
    double VerticalViewSize);
/// <summary>滚动百分比设置参数。</summary>
/// <param name="HorizontalPercent">水平滚动百分比。</param>
/// <param name="VerticalPercent">垂直滚动百分比。</param>
public sealed record SetScrollPercentSpec(double HorizontalPercent, double VerticalPercent);
/// <summary>滚动步进动作参数。</summary>
/// <param name="HorizontalAmount">水平滚动步进枚举名。有效值：LargeDecrement、SmallDecrement、NoAmount、LargeIncrement、SmallIncrement。</param>
/// <param name="VerticalAmount">垂直滚动步进枚举名。有效值：LargeDecrement、SmallDecrement、NoAmount、LargeIncrement、SmallIncrement。</param>
public sealed record ScrollActionSpec(string HorizontalAmount, string VerticalAmount);
/// <summary>WindowPattern 状态响应。</summary>
/// <param name="CanMaximize">是否可最大化。</param>
/// <param name="CanMinimize">是否可最小化。</param>
/// <param name="IsModal">是否模态窗口。</param>
/// <param name="IsTopmost">是否置顶。</param>
/// <param name="WindowInteractionState">交互状态。</param>
/// <param name="WindowVisualState">可视状态。</param>
public sealed record WindowPatternStateResult(
    bool CanMaximize,
    bool CanMinimize,
    bool IsModal,
    bool IsTopmost,
    string WindowInteractionState,
    string WindowVisualState);
/// <summary>窗口可视状态设置参数。</summary>
/// <param name="State">窗口状态枚举名。有效值：Normal、Maximized、Minimized。</param>
public sealed record SetWindowVisualStateSpec(string State);
/// <summary>窗口空闲等待参数。</summary>
/// <param name="Milliseconds">等待时长（毫秒）。</param>
public sealed record WaitForInputIdleSpec(int Milliseconds);
/// <summary>Selection2Pattern 状态响应。</summary>
/// <param name="ItemCount">选中项数量。</param>
/// <param name="CurrentSelectedItemElementId">当前选中项元素 ID。</param>
/// <param name="FirstSelectedItemElementId">首个选中项元素 ID。</param>
/// <param name="LastSelectedItemElementId">最后选中项元素 ID。</param>
public sealed record Selection2PatternStateResult(int ItemCount, string? CurrentSelectedItemElementId, string? FirstSelectedItemElementId, string? LastSelectedItemElementId);
/// <summary>SelectionItemPattern 状态响应。</summary>
/// <param name="IsSelected">当前元素是否被选中。</param>
/// <param name="SelectionContainerElementId">所属选择容器元素 ID。</param>
public sealed record SelectionItemPatternStateResult(bool IsSelected, string? SelectionContainerElementId);
/// <summary>SelectionItem 动作参数。</summary>
/// <param name="Action">动作：select/add/remove。</param>
public sealed record SelectionItemActionSpec(string Action);
/// <summary>DockPattern 状态响应。</summary>
/// <param name="DockPosition">当前停靠位置。</param>
public sealed record DockPatternStateResult(string DockPosition);
/// <summary>Dock 位置设置参数。</summary>
/// <param name="Position">停靠位置枚举名。有效值：Top、Left、Bottom、Right、Fill、None。</param>
public sealed record SetDockSpec(string Position);
/// <summary>TransformPattern 状态响应。</summary>
/// <param name="CanMove">是否支持移动。</param>
/// <param name="CanResize">是否支持调整尺寸。</param>
/// <param name="CanRotate">是否支持旋转。</param>
public sealed record TransformPatternStateResult(bool CanMove, bool CanResize, bool CanRotate);
/// <summary>Transform 动作参数。</summary>
/// <param name="Action">动作：move/resize/rotate。</param>
/// <param name="X">移动目标 X。</param>
/// <param name="Y">移动目标 Y。</param>
/// <param name="Width">目标宽度。</param>
/// <param name="Height">目标高度。</param>
/// <param name="Degrees">旋转角度。</param>
public sealed record TransformActionSpec(string Action, double? X, double? Y, double? Width, double? Height, double? Degrees);
/// <summary>Transform2Pattern 状态响应。</summary>
/// <param name="CanMove">是否支持移动。</param>
/// <param name="CanResize">是否支持调整尺寸。</param>
/// <param name="CanRotate">是否支持旋转。</param>
/// <param name="CanZoom">是否支持缩放。</param>
/// <param name="ZoomLevel">当前缩放级别。</param>
/// <param name="ZoomMinimum">最小缩放级别。</param>
/// <param name="ZoomMaximum">最大缩放级别。</param>
public sealed record Transform2PatternStateResult(bool CanMove, bool CanResize, bool CanRotate, bool CanZoom, double ZoomLevel, double ZoomMinimum, double ZoomMaximum);
/// <summary>Transform2 动作参数。</summary>
/// <param name="Action">动作：zoom/zoom-by-unit。</param>
/// <param name="Zoom">目标缩放值。</param>
/// <param name="ZoomUnit">缩放单位枚举名。有效值：NoAmount、LargeDecrement、SmallDecrement、LargeIncrement、SmallIncrement。</param>
public sealed record Transform2ActionSpec(string Action, double? Zoom, string? ZoomUnit);
/// <summary>GridPattern 状态响应。</summary>
/// <param name="RowCount">行数。</param>
/// <param name="ColumnCount">列数。</param>
public sealed record GridPatternStateResult(int RowCount, int ColumnCount);
/// <summary>Grid 单元格坐标参数。</summary>
/// <param name="Row">行索引。</param>
/// <param name="Column">列索引。</param>
public sealed record GridItemSpec(int Row, int Column);
/// <summary>GridItemPattern 状态响应。</summary>
/// <param name="Row">所在行。</param>
/// <param name="RowSpan">跨行数。</param>
/// <param name="Column">所在列。</param>
/// <param name="ColumnSpan">跨列数。</param>
/// <param name="ContainingGridElementId">所属 Grid 元素 ID。</param>
public sealed record GridItemPatternStateResult(int Row, int RowSpan, int Column, int ColumnSpan, string? ContainingGridElementId);
/// <summary>TablePattern 状态响应。</summary>
/// <param name="RowOrColumnMajor">主次方向信息。</param>
/// <param name="ColumnHeaderElementIds">列头元素 ID 列表。</param>
/// <param name="RowHeaderElementIds">行头元素 ID 列表。</param>
public sealed record TablePatternStateResult(string RowOrColumnMajor, IReadOnlyList<string> ColumnHeaderElementIds, IReadOnlyList<string> RowHeaderElementIds);
/// <summary>TableItemPattern 状态响应。</summary>
/// <param name="ColumnHeaderItemElementIds">列头项元素 ID 列表。</param>
/// <param name="RowHeaderItemElementIds">行头项元素 ID 列表。</param>
public sealed record TableItemPatternStateResult(IReadOnlyList<string> ColumnHeaderItemElementIds, IReadOnlyList<string> RowHeaderItemElementIds);
/// <summary>MultipleViewPattern 状态响应。</summary>
/// <param name="CurrentView">当前视图 ID。</param>
/// <param name="SupportedViews">支持的视图 ID 列表。</param>
/// <param name="ViewNames">视图 ID 到名称的映射。</param>
public sealed record MultipleViewPatternStateResult(int CurrentView, IReadOnlyList<int> SupportedViews, IReadOnlyDictionary<string, string> ViewNames);
/// <summary>切换视图参数。</summary>
/// <param name="View">目标视图 ID。</param>
public sealed record SetMultipleViewSpec(int View);
/// <summary>ItemContainer 查找参数。</summary>
/// <param name="StartAfterElementId">从哪个元素之后开始查找。</param>
/// <param name="PropertyName">属性名（PropertyId.Name）。</param>
/// <param name="Value">目标属性值。</param>
public sealed record ItemContainerFindSpec(string? StartAfterElementId, string? PropertyName, object? Value);
/// <summary>Spreadsheet 按名称查找参数。</summary>
/// <param name="Name">单元格名称（如 A1）。</param>
public sealed record SpreadsheetItemByNameSpec(string Name);
/// <summary>SpreadsheetItemPattern 状态响应。</summary>
/// <param name="Formula">公式文本。</param>
/// <param name="AnnotationObjectElementIds">批注对象元素 ID 列表。</param>
/// <param name="AnnotationTypes">批注类型列表。</param>
public sealed record SpreadsheetItemPatternStateResult(string? Formula, IReadOnlyList<string> AnnotationObjectElementIds, IReadOnlyList<string> AnnotationTypes);
/// <summary>StylesPattern 状态响应。</summary>
/// <param name="ExtendedProperties">扩展属性文本。</param>
/// <param name="FillColor">填充色。</param>
/// <param name="FillPatternColor">填充图案颜色。</param>
/// <param name="FillPatternStyle">填充图案样式。</param>
/// <param name="Shape">形状描述。</param>
/// <param name="Style">样式枚举值。</param>
/// <param name="StyleName">样式名称。</param>
public sealed record StylesPatternStateResult(string? ExtendedProperties, int FillColor, int FillPatternColor, string? FillPatternStyle, string? Shape, string Style, string? StyleName);
/// <summary>ObjectModelPattern 状态响应。</summary>
/// <param name="UnderlyingObjectString">底层对象字符串表示。</param>
/// <param name="UnderlyingObjectType">底层对象类型全名。</param>
public sealed record ObjectModelPatternStateResult(string UnderlyingObjectString, string UnderlyingObjectType);
/// <summary>LegacyIAccessiblePattern 状态响应。</summary>
/// <param name="ChildId">子对象 ID。</param>
/// <param name="DefaultAction">默认动作文本。</param>
/// <param name="Description">描述文本。</param>
/// <param name="Help">帮助文本。</param>
/// <param name="KeyboardShortcut">快捷键。</param>
/// <param name="Name">名称。</param>
/// <param name="Role">辅助功能角色。</param>
/// <param name="SelectionElementIds">选中元素 ID 列表。</param>
/// <param name="State">辅助功能状态。</param>
/// <param name="Value">值文本。</param>
public sealed record LegacyIAccessiblePatternStateResult(
    int ChildId,
    string? DefaultAction,
    string? Description,
    string? Help,
    string? KeyboardShortcut,
    string? Name,
    string Role,
    IReadOnlyList<string> SelectionElementIds,
    string State,
    string? Value);
/// <summary>LegacyIAccessible 动作参数。</summary>
/// <param name="Action">动作：default/select/set-value。</param>
/// <param name="FlagsSelect">Select 所需 flags。</param>
/// <param name="Value">set-value 的目标值。</param>
public sealed record LegacyIAccessibleActionSpec(string Action, int? FlagsSelect, string? Value);
/// <summary>AnnotationPattern 状态响应。</summary>
/// <param name="AnnotationType">批注类型。</param>
/// <param name="AnnotationTypeName">批注类型名称。</param>
/// <param name="Author">作者。</param>
/// <param name="DateTime">时间文本。</param>
/// <param name="TargetElementId">目标元素 ID。</param>
public sealed record AnnotationPatternStateResult(string AnnotationType, string? AnnotationTypeName, string? Author, string? DateTime, string? TargetElementId);
/// <summary>DragPattern 状态响应。</summary>
/// <param name="DropEffect">当前拖放效果。</param>
/// <param name="DropEffects">支持拖放效果列表。</param>
/// <param name="IsGrabbed">是否已抓取。</param>
/// <param name="GrabbedItemElementIds">已抓取元素 ID 列表。</param>
public sealed record DragPatternStateResult(string? DropEffect, IReadOnlyList<string> DropEffects, bool IsGrabbed, IReadOnlyList<string> GrabbedItemElementIds);
/// <summary>DropTargetPattern 状态响应。</summary>
/// <param name="DropTargetEffect">当前目标拖放效果。</param>
/// <param name="DropTargetEffects">支持目标拖放效果列表。</param>
public sealed record DropTargetPatternStateResult(string? DropTargetEffect, IReadOnlyList<string> DropTargetEffects);
/// <summary>SynchronizedInput 动作参数。</summary>
/// <param name="Action">动作：start/cancel。</param>
/// <param name="InputType">输入类型枚举名。有效值：KeyUp、KeyDown、LeftMouseUp、LeftMouseDown、RightMouseUp、RightMouseDown。</param>
public sealed record SynchronizedInputActionSpec(string Action, string? InputType);
/// <summary>TextPattern 状态响应。</summary>
/// <param name="SupportedTextSelection">支持的文本选区模式。</param>
/// <param name="DocumentText">文档全文。</param>
/// <param name="SelectionTexts">选区文本列表。</param>
/// <param name="VisibleRangeTexts">可见范围文本列表。</param>
public sealed record TextPatternStateResult(string SupportedTextSelection, string DocumentText, IReadOnlyList<string> SelectionTexts, IReadOnlyList<string> VisibleRangeTexts);
/// <summary>Text2 光标范围响应。</summary>
/// <param name="IsActive">是否激活。</param>
/// <param name="CaretText">光标范围文本。</param>
public sealed record Text2CaretRangeResult(bool IsActive, string CaretText);
/// <summary>TextEditPattern 状态响应。</summary>
/// <param name="ActiveCompositionText">活动组合文本。</param>
/// <param name="ConversionTargetText">转换目标文本。</param>
public sealed record TextEditPatternStateResult(string ActiveCompositionText, string ConversionTargetText);
/// <summary>TextChildPattern 状态响应。</summary>
/// <param name="TextContainerElementId">文本容器元素 ID。</param>
/// <param name="Text">文本内容。</param>
public sealed record TextChildPatternStateResult(string? TextContainerElementId, string Text);

/// <summary>矩形坐标结构。</summary>
/// <param name="X">左上角 X。</param>
/// <param name="Y">左上角 Y。</param>
/// <param name="Width">宽度。</param>
/// <param name="Height">高度。</param>
public sealed record RectBounds(int X, int Y, int Width, int Height);

/// <summary>元素基础信息响应。</summary>
/// <param name="ElementId">元素标识。</param>
/// <param name="Name">元素名称。</param>
/// <param name="AutomationId">自动化 ID。</param>
/// <param name="ClassName">类名。</param>
/// <param name="ControlType">控件类型。</param>
/// <param name="FrameworkType">框架类型。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="IsOffscreen">是否离屏。</param>
/// <param name="IsAvailable">是否仍可用。</param>
/// <param name="Bounds">边界矩形。</param>
public sealed record ElementInfoResult(
    string ElementId,
    string Name,
    string AutomationId,
    string ClassName,
    string ControlType,
    string FrameworkType,
    bool IsEnabled,
    bool IsOffscreen,
    bool IsAvailable,
    RectBounds Bounds);

/// <summary>元素类型转换请求参数。</summary>
/// <param name="Type">目标控件类型名（如 TextBox/Window）。</param>
public sealed record AsTypeSpec(string? Type);
