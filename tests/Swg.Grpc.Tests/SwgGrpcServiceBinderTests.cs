using Grpc.Core.Interceptors;
using Xunit;

namespace Swg.Grpc.Tests;

public class SwgGrpcServiceBinderTests
{
    [Fact]
    public void GetServiceDefinitions_WithoutInterceptor_Returns7Definitions()
    {
        var definitions = SwgGrpcServiceBinder.GetServiceDefinitions().ToList();
        Assert.Equal(7, definitions.Count);
    }

    [Fact]
    public void GetServiceDefinitions_WithInterceptor_Returns7Definitions()
    {
        var mockInterceptor = new MockInterceptor();
        var definitions = SwgGrpcServiceBinder.GetServiceDefinitions(mockInterceptor).ToList();
        Assert.Equal(7, definitions.Count);
    }

    [Fact]
    public void GetServiceDefinitions_AllDefinitionsNotNull()
    {
        var definitions = SwgGrpcServiceBinder.GetServiceDefinitions().ToList();
        foreach (var def in definitions)
        {
            Assert.NotNull(def);
        }
    }

    private sealed class MockInterceptor : Interceptor { }
}
