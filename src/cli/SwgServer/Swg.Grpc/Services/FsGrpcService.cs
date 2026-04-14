using Grpc.Core;
using Swg.Grpc.Api;
using Swg.Grpc.Fs;

namespace Swg.Grpc.Services;

public sealed class FsGrpcService : FsService.FsServiceBase
{
    public override Task<ReadTextResponse> ReadText(ReadTextRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.ReadText(request)));

    public override Task<WriteTextResponse> WriteText(WriteTextRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.WriteText(request)));

    public override Task<AppendTextResponse> AppendText(AppendTextRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.AppendText(request)));

    public override Task<CopyFileResponse> CopyFile(CopyFileRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.CopyFile(request)));

    public override Task<MoveFileResponse> MoveFile(MoveFileRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.MoveFile(request)));

    public override Task<DeleteFileResponse> DeleteFile(DeleteFileRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.DeleteFile(request)));

    public override Task<ExistsResponse> Exists(ExistsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.Exists(request)));

    public override Task<GetItemInfoResponse> GetItemInfo(GetItemInfoRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.GetItemInfo(request)));

    public override Task<CreateDirectoryResponse> CreateDirectory(CreateDirectoryRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.CreateDirectory(request)));

    public override Task<DeleteDirectoryResponse> DeleteDirectory(DeleteDirectoryRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.DeleteDirectory(request)));

    public override Task<ListDirectoryResponse> ListDirectory(ListDirectoryRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.ListDirectory(request)));

    public override Task<MoveDirectoryResponse> MoveDirectory(MoveDirectoryRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.MoveDirectory(request)));

    public override Task<SearchFilesResponse> SearchFiles(SearchFilesRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.SearchFiles(request)));

    public override Task<FindExeShortcutsResponse> FindExeShortcuts(FindExeShortcutsRequest request, ServerCallContext context) =>
        GrpcRouteRunner.RunAsync(() => Task.FromResult(SwgGrpcFsApi.FindExeShortcuts(request)));
}
