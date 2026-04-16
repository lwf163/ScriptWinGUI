using Google.Protobuf.WellKnownTypes;
using Swg.Fs;
using Swg.Grpc.Fs;

namespace Swg.Grpc.Api;

/// <summary>
/// 文件系统 gRPC 门面：封装 <c>Swg.Fs</c> 能力，与对应 Proto RPC 语义一致。
/// <para>
/// 提供文件读写、复制/移动/删除、目录操作、文件搜索等文件系统操作。
/// 所有方法均为无状态静态方法，线程安全。
/// </para>
/// <para>对应 Proto 服务：<c>swg.fs.FsService</c></para>
/// <para>
/// 异常映射约定（经由 <see cref="GrpcRouteRunner"/> 统一处理）：
/// <list type="bullet">
///   <item><description><see cref="ArgumentException"/> → <c>StatusCode.InvalidArgument</c></description></item>
///   <item><description><see cref="InvalidOperationException"/> → <c>StatusCode.Unavailable</c></description></item>
///   <item><description>其他异常 → <c>StatusCode.Internal</c></description></item>
/// </list>
/// </para>
/// </summary>
public static class SwgGrpcFsApi
{
    /// <summary>
    /// 读取文本文件的完整内容。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Path</c>（string，必填）：文件路径</description></item>
    ///   <item><description><c>Encoding</c>（string，可选）：文本编码名称（如 <c>utf-8</c>、<c>gb2312</c>），为空使用默认编码</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ReadTextResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Text</c>（string）：文件文本内容</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Path</c> 为空</exception>
    public static ReadTextResponse ReadText(ReadTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        string text = SwgFs.ReadText(path, request.Encoding);
        return new ReadTextResponse { Text = text };
    }

    /// <summary>
    /// 将文本写入文件（覆盖已有内容）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Path</c>（string，必填）：文件路径</description></item>
    ///   <item><description><c>Text</c>（string，必填）：要写入的文本内容</description></item>
    ///   <item><description><c>Encoding</c>（string，可选）：文本编码名称</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="WriteTextResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Success</c>（bool）：是否写入成功</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Path</c> 或 <c>Text</c> 为空</exception>
    public static WriteTextResponse WriteText(WriteTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        if (request.Text is null)
            throw new ArgumentException("Text 必填。");
        SwgFs.WriteText(path, request.Text, request.Encoding);
        return new WriteTextResponse { Success = true };
    }

    /// <summary>
    /// 将文本追加到文件末尾（文件不存在时自动创建）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Path</c>（string，必填）：文件路径</description></item>
    ///   <item><description><c>Text</c>（string，必填）：要追加的文本内容</description></item>
    ///   <item><description><c>Encoding</c>（string，可选）：文本编码名称</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="AppendTextResponse"/>，<c>Success</c> 表示是否追加成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Path</c> 或 <c>Text</c> 为空</exception>
    public static AppendTextResponse AppendText(AppendTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        if (request.Text is null)
            throw new ArgumentException("Text 必填。");
        SwgFs.AppendText(path, request.Text, request.Encoding);
        return new AppendTextResponse { Success = true };
    }

    /// <summary>
    /// 复制文件到目标路径。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SourcePath</c>（string，必填）：源文件路径</description></item>
    ///   <item><description><c>TargetPath</c>（string，必填）：目标文件路径</description></item>
    ///   <item><description><c>Overwrite</c>（bool）：是否覆盖已存在的目标文件</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="CopyFileResponse"/>，<c>Success</c> 表示是否复制成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>SourcePath</c> 或 <c>TargetPath</c> 为空</exception>
    public static CopyFileResponse CopyFile(CopyFileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string source = RequirePath(request.SourcePath, nameof(request.SourcePath));
        string target = RequirePath(request.TargetPath, nameof(request.TargetPath));
        SwgFs.CopyFile(source, target, request.Overwrite);
        return new CopyFileResponse { Success = true };
    }

    /// <summary>
    /// 移动文件到目标路径。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SourcePath</c>（string，必填）：源文件路径</description></item>
    ///   <item><description><c>TargetPath</c>（string，必填）：目标文件路径</description></item>
    ///   <item><description><c>Overwrite</c>（bool）：是否覆盖已存在的目标文件</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="MoveFileResponse"/>，<c>Success</c> 表示是否移动成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>SourcePath</c> 或 <c>TargetPath</c> 为空</exception>
    public static MoveFileResponse MoveFile(MoveFileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string source = RequirePath(request.SourcePath, nameof(request.SourcePath));
        string target = RequirePath(request.TargetPath, nameof(request.TargetPath));
        SwgFs.MoveFile(source, target, request.Overwrite);
        return new MoveFileResponse { Success = true };
    }

    /// <summary>
    /// 删除指定文件。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Path</c>（string，必填）：要删除的文件路径</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="DeleteFileResponse"/>，<c>Success</c> 表示是否删除成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Path</c> 为空</exception>
    public static DeleteFileResponse DeleteFile(DeleteFileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        SwgFs.DeleteFile(path);
        return new DeleteFileResponse { Success = true };
    }

    /// <summary>
    /// 检查指定路径的文件或目录是否存在。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Path</c>（string，必填）：要检查的路径</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ExistsResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Exists</c>（bool）：路径是否存在</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Path</c> 为空</exception>
    public static ExistsResponse Exists(ExistsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        return new ExistsResponse { Exists = SwgFs.Exists(path) };
    }

    /// <summary>
    /// 获取文件或目录的详细信息（大小、时间戳、属性等）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Path</c>（string，必填）：目标路径</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="GetItemInfoResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Name</c>（string）：文件/目录名称</description></item>
    ///   <item><description><c>FullPath</c>（string）：完整路径</description></item>
    ///   <item><description><c>IsDirectory</c>（bool）：是否为目录</description></item>
    ///   <item><description><c>SizeBytes</c>（int64）：文件大小字节数（<c>HasSizeBytes</c> 为 true 时有效，目录无此字段）</description></item>
    ///   <item><description><c>CreationTimeUtc</c>（Timestamp）：创建时间（UTC）</description></item>
    ///   <item><description><c>LastWriteTimeUtc</c>（Timestamp）：最后修改时间（UTC）</description></item>
    ///   <item><description><c>LastAccessTimeUtc</c>（Timestamp）：最后访问时间（UTC）</description></item>
    ///   <item><description><c>Attributes</c>（string）：文件属性字符串</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Path</c> 为空</exception>
    public static GetItemInfoResponse GetItemInfo(GetItemInfoRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        SwgFs.GetItemInfoResult info = SwgFs.GetItemInfo(path);
        var r = new GetItemInfoResponse
        {
            Name = info.name,
            FullPath = info.fullPath,
            IsDirectory = info.isDirectory,
            Attributes = info.attributes,
        };
        if (info.sizeBytes.HasValue)
        {
            r.HasSizeBytes = true;
            r.SizeBytes = info.sizeBytes.Value;
        }

        r.CreationTimeUtc = Timestamp.FromDateTime(DateTime.SpecifyKind(info.creationTimeUtc, DateTimeKind.Utc));
        r.LastWriteTimeUtc = Timestamp.FromDateTime(DateTime.SpecifyKind(info.lastWriteTimeUtc, DateTimeKind.Utc));
        r.LastAccessTimeUtc = Timestamp.FromDateTime(DateTime.SpecifyKind(info.lastAccessTimeUtc, DateTimeKind.Utc));
        return r;
    }

    /// <summary>
    /// 创建目录（递归创建所有不存在的父目录）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Path</c>（string，必填）：要创建的目录路径</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="CreateDirectoryResponse"/>，<c>Success</c> 表示是否创建成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Path</c> 为空</exception>
    public static CreateDirectoryResponse CreateDirectory(CreateDirectoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        SwgFs.CreateDirectory(path);
        return new CreateDirectoryResponse { Success = true };
    }

    /// <summary>
    /// 删除目录（递归删除所有子目录和文件）。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Path</c>（string，必填）：要删除的目录路径</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="DeleteDirectoryResponse"/>，<c>Success</c> 表示是否删除成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Path</c> 为空</exception>
    public static DeleteDirectoryResponse DeleteDirectory(DeleteDirectoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        SwgFs.DeleteDirectory(path);
        return new DeleteDirectoryResponse { Success = true };
    }

    /// <summary>
    /// 列出目录下的文件和子目录名称。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>Path</c>（string，必填）：目录路径</description></item>
    ///   <item><description><c>Pattern</c>（string，可选）：搜索模式（如 <c>*.txt</c>），为空则返回所有</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="ListDirectoryResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Names</c>（repeated string）：文件和目录名称列表</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>Path</c> 为空</exception>
    public static ListDirectoryResponse ListDirectory(ListDirectoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        IReadOnlyList<string> names = SwgFs.ListDirectory(path, request.Pattern);
        var r = new ListDirectoryResponse();
        r.Names.AddRange(names);
        return r;
    }

    /// <summary>
    /// 移动目录到目标路径。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>SourcePath</c>（string，必填）：源目录路径</description></item>
    ///   <item><description><c>TargetPath</c>（string，必填）：目标目录路径</description></item>
    ///   <item><description><c>Overwrite</c>（bool）：是否覆盖已存在的目标目录</description></item>
    /// </list>
    /// </param>
    /// <returns><see cref="MoveDirectoryResponse"/>，<c>Success</c> 表示是否移动成功</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>SourcePath</c> 或 <c>TargetPath</c> 为空</exception>
    public static MoveDirectoryResponse MoveDirectory(MoveDirectoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string source = RequirePath(request.SourcePath, nameof(request.SourcePath));
        string target = RequirePath(request.TargetPath, nameof(request.TargetPath));
        SwgFs.MoveDirectory(source, target, request.Overwrite);
        return new MoveDirectoryResponse { Success = true };
    }

    /// <summary>
    /// 在指定根目录下递归搜索匹配模式的文件。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>RootPath</c>（string，必填）：搜索根目录</description></item>
    ///   <item><description><c>Pattern</c>（string，可选）：搜索模式（如 <c>*.txt</c>），为空则匹配所有文件</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="SearchFilesResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>Paths</c>（repeated string）：匹配文件的完整路径列表</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>RootPath</c> 为空</exception>
    public static SearchFilesResponse SearchFiles(SearchFilesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string root = RequirePath(request.RootPath, nameof(request.RootPath));
        IReadOnlyList<string> paths = SwgFs.SearchFiles(root, request.Pattern);
        var r = new SearchFilesResponse();
        r.Paths.AddRange(paths);
        return r;
    }

    /// <summary>
    /// 在系统常见位置（桌面、开始菜单等）查找指定可执行文件的快捷方式。
    /// </summary>
    /// <param name="request">
    /// 请求参数：
    /// <list type="bullet">
    ///   <item><description><c>ExeName</c>（string，必填）：可执行文件名（如 <c>chrome.exe</c>）</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// <see cref="FindExeShortcutsResponse"/> 包含：
    /// <list type="bullet">
    ///   <item><description><c>TargetPaths</c>（repeated string）：找到的快捷方式目标路径列表</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> 为 null</exception>
    /// <exception cref="ArgumentException"><c>ExeName</c> 为空</exception>
    public static FindExeShortcutsResponse FindExeShortcuts(FindExeShortcutsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.ExeName))
            throw new ArgumentException("ExeName 必填。");
        IReadOnlyList<string> targets = SwgFs.FindExeShortcuts(request.ExeName);
        var r = new FindExeShortcutsResponse();
        r.TargetPaths.AddRange(targets);
        return r;
    }

    private static string RequirePath(string? path, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException($"{fieldName} 必填。");
        return path.Trim();
    }
}
