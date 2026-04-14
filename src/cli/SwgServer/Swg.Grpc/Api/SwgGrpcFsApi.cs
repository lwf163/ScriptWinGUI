using Google.Protobuf.WellKnownTypes;
using Swg.Fs;
using Swg.Grpc.Fs;

namespace Swg.Grpc.Api;

/// <summary>
/// 文件系统 gRPC 门面：封装 <c>Swg.Fs</c> 能力，与对应 Proto RPC 语义一致。
/// </summary>
public static class SwgGrpcFsApi
{
    public static ReadTextResponse ReadText(ReadTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        string text = SwgFs.ReadText(path, request.Encoding);
        return new ReadTextResponse { Text = text };
    }

    public static WriteTextResponse WriteText(WriteTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        if (request.Text is null)
            throw new ArgumentException("Text 必填。");
        SwgFs.WriteText(path, request.Text, request.Encoding);
        return new WriteTextResponse { Success = true };
    }

    public static AppendTextResponse AppendText(AppendTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        if (request.Text is null)
            throw new ArgumentException("Text 必填。");
        SwgFs.AppendText(path, request.Text, request.Encoding);
        return new AppendTextResponse { Success = true };
    }

    public static CopyFileResponse CopyFile(CopyFileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string source = RequirePath(request.SourcePath, nameof(request.SourcePath));
        string target = RequirePath(request.TargetPath, nameof(request.TargetPath));
        SwgFs.CopyFile(source, target, request.Overwrite);
        return new CopyFileResponse { Success = true };
    }

    public static MoveFileResponse MoveFile(MoveFileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string source = RequirePath(request.SourcePath, nameof(request.SourcePath));
        string target = RequirePath(request.TargetPath, nameof(request.TargetPath));
        SwgFs.MoveFile(source, target, request.Overwrite);
        return new MoveFileResponse { Success = true };
    }

    public static DeleteFileResponse DeleteFile(DeleteFileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        SwgFs.DeleteFile(path);
        return new DeleteFileResponse { Success = true };
    }

    public static ExistsResponse Exists(ExistsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        return new ExistsResponse { Exists = SwgFs.Exists(path) };
    }

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

    public static CreateDirectoryResponse CreateDirectory(CreateDirectoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        SwgFs.CreateDirectory(path);
        return new CreateDirectoryResponse { Success = true };
    }

    public static DeleteDirectoryResponse DeleteDirectory(DeleteDirectoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        SwgFs.DeleteDirectory(path);
        return new DeleteDirectoryResponse { Success = true };
    }

    public static ListDirectoryResponse ListDirectory(ListDirectoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = RequirePath(request.Path, nameof(request.Path));
        IReadOnlyList<string> names = SwgFs.ListDirectory(path, request.Pattern);
        var r = new ListDirectoryResponse();
        r.Names.AddRange(names);
        return r;
    }

    public static MoveDirectoryResponse MoveDirectory(MoveDirectoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string source = RequirePath(request.SourcePath, nameof(request.SourcePath));
        string target = RequirePath(request.TargetPath, nameof(request.TargetPath));
        SwgFs.MoveDirectory(source, target, request.Overwrite);
        return new MoveDirectoryResponse { Success = true };
    }

    public static SearchFilesResponse SearchFiles(SearchFilesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string root = RequirePath(request.RootPath, nameof(request.RootPath));
        IReadOnlyList<string> paths = SwgFs.SearchFiles(root, request.Pattern);
        var r = new SearchFilesResponse();
        r.Paths.AddRange(paths);
        return r;
    }

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
