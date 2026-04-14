using Swg.Grpc.Api;
using Swg.Grpc.Fs;
using Xunit;

namespace Swg.Grpc.Tests.Api;

public class SwgGrpcFsApiTests
{
    [Fact]
    public void ReadText_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFsApi.ReadText(null!));
    }

    [Fact]
    public void ReadText_EmptyPath_ThrowsArgumentException()
    {
        var request = new ReadTextRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.ReadText(request));
        Assert.Contains("Path", ex.Message);
    }

    [Fact]
    public void WriteText_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFsApi.WriteText(null!));
    }

    [Fact]
    public void WriteText_EmptyPath_ThrowsArgumentException()
    {
        var request = new WriteTextRequest { Text = "hello" };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.WriteText(request));
        Assert.Contains("Path", ex.Message);
    }

    [Fact]
    public void CopyFile_NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SwgGrpcFsApi.CopyFile(null!));
    }

    [Fact]
    public void CopyFile_EmptySource_ThrowsArgumentException()
    {
        var request = new CopyFileRequest { TargetPath = "C:\\dest" };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.CopyFile(request));
        Assert.Contains("SourcePath", ex.Message);
    }

    [Fact]
    public void MoveFile_EmptyTarget_ThrowsArgumentException()
    {
        var request = new MoveFileRequest { SourcePath = "C:\\src" };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.MoveFile(request));
        Assert.Contains("TargetPath", ex.Message);
    }

    [Fact]
    public void DeleteFile_EmptyPath_ThrowsArgumentException()
    {
        var request = new DeleteFileRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.DeleteFile(request));
        Assert.Contains("Path", ex.Message);
    }

    [Fact]
    public void Exists_EmptyPath_ThrowsArgumentException()
    {
        var request = new ExistsRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.Exists(request));
        Assert.Contains("Path", ex.Message);
    }

    [Fact]
    public void GetItemInfo_EmptyPath_ThrowsArgumentException()
    {
        var request = new GetItemInfoRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.GetItemInfo(request));
        Assert.Contains("Path", ex.Message);
    }

    [Fact]
    public void CreateDirectory_EmptyPath_ThrowsArgumentException()
    {
        var request = new CreateDirectoryRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.CreateDirectory(request));
        Assert.Contains("Path", ex.Message);
    }

    [Fact]
    public void DeleteDirectory_EmptyPath_ThrowsArgumentException()
    {
        var request = new DeleteDirectoryRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.DeleteDirectory(request));
        Assert.Contains("Path", ex.Message);
    }

    [Fact]
    public void ListDirectory_EmptyPath_ThrowsArgumentException()
    {
        var request = new ListDirectoryRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.ListDirectory(request));
        Assert.Contains("Path", ex.Message);
    }

    [Fact]
    public void MoveDirectory_EmptySource_ThrowsArgumentException()
    {
        var request = new MoveDirectoryRequest { TargetPath = "C:\\dest" };
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.MoveDirectory(request));
        Assert.Contains("SourcePath", ex.Message);
    }

    [Fact]
    public void SearchFiles_EmptyRoot_ThrowsArgumentException()
    {
        var request = new SearchFilesRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.SearchFiles(request));
        Assert.Contains("RootPath", ex.Message);
    }

    [Fact]
    public void FindExeShortcuts_EmptyExeName_ThrowsArgumentException()
    {
        var request = new FindExeShortcutsRequest();
        var ex = Assert.Throws<ArgumentException>(() => SwgGrpcFsApi.FindExeShortcuts(request));
        Assert.Contains("ExeName", ex.Message);
    }
}
