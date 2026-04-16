using Grpc.Core;
using Swg.Grpc.Api;
using Swg.Grpc.Fs;

namespace Swg.Grpc.Services;

/// <summary>
/// 文件系统 gRPC 服务实现：提供文件读写、复制/移动/删除、目录操作、文件搜索等文件系统操作。
/// <para>
/// 继承自 <c>FsService.FsServiceBase</c>，由 gRPC 运行时自动注册。
/// 所有 RPC 均通过 <see cref="GrpcRouteRunner"/> 统一异常映射。
/// </para>
/// <para>对应 Proto 定义：<c>swg.fs.FsService</c></para>
/// </summary>
public sealed class FsGrpcService : FsService.FsServiceBase
{
    /// <summary>读取文本文件的完整内容。</summary>
    public override Task<ReadTextResponse> ReadText(ReadTextRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.ReadText(request)));

    /// <summary>将文本写入文件（覆盖已有内容）。</summary>
    public override Task<WriteTextResponse> WriteText(WriteTextRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.WriteText(request)));

    /// <summary>将文本追加到文件末尾。</summary>
    public override Task<AppendTextResponse> AppendText(AppendTextRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.AppendText(request)));

    /// <summary>复制文件到目标路径。</summary>
    public override Task<CopyFileResponse> CopyFile(CopyFileRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.CopyFile(request)));

    /// <summary>移动文件到目标路径。</summary>
    public override Task<MoveFileResponse> MoveFile(MoveFileRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.MoveFile(request)));

    /// <summary>删除指定文件。</summary>
    public override Task<DeleteFileResponse> DeleteFile(DeleteFileRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.DeleteFile(request)));

    /// <summary>检查文件或目录是否存在。</summary>
    public override Task<ExistsResponse> Exists(ExistsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.Exists(request)));

    /// <summary>获取文件或目录的详细信息。</summary>
    public override Task<GetItemInfoResponse> GetItemInfo(GetItemInfoRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.GetItemInfo(request)));

    /// <summary>创建目录（递归创建所有不存在的父目录）。</summary>
    public override Task<CreateDirectoryResponse> CreateDirectory(CreateDirectoryRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.CreateDirectory(request)));

    /// <summary>删除目录（递归删除所有子目录和文件）。</summary>
    public override Task<DeleteDirectoryResponse> DeleteDirectory(DeleteDirectoryRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.DeleteDirectory(request)));

    /// <summary>列出目录下的文件和子目录名称。</summary>
    public override Task<ListDirectoryResponse> ListDirectory(ListDirectoryRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.ListDirectory(request)));

    /// <summary>移动目录到目标路径。</summary>
    public override Task<MoveDirectoryResponse> MoveDirectory(MoveDirectoryRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.MoveDirectory(request)));

    /// <summary>在指定根目录下递归搜索匹配模式的文件。</summary>
    public override Task<SearchFilesResponse> SearchFiles(SearchFilesRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.SearchFiles(request)));

    /// <summary>在系统常见位置查找指定可执行文件的快捷方式。</summary>
    public override Task<FindExeShortcutsResponse> FindExeShortcuts(FindExeShortcutsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.FindExeShortcuts(request)));
}
