using ApiGateway.Observability;
using Xunit;

namespace ApiGateway.Tests.Observability;

public class ObservabilityOptionsTests
{
    [Fact]
    public void Defaults_Are_Sensible()
    {
        var options = new ObservabilityOptions();

        Assert.Equal("nur-tricenter-api-gateway", options.ServiceName);
        Assert.True(options.TracingEnabled);
        Assert.Equal("http://jaeger:9411/api/v2/spans", options.ZipkinEndpoint);
    }

    [Fact]
    public void FromEnvironment_Defaults_When_Not_Set()
    {
        Environment.SetEnvironmentVariable("SERVICE_NAME", null);
        Environment.SetEnvironmentVariable("ZIPKIN_ENDPOINT", null);
        Environment.SetEnvironmentVariable("TRACING_ENABLED", null);

        var options = ObservabilityOptions.FromEnvironment();

        Assert.Equal("nur-tricenter-api-gateway", options.ServiceName);
        Assert.True(options.TracingEnabled);
    }

    [Fact]
    public void FromEnvironment_Reads_Tracing_Disabled()
    {
        Environment.SetEnvironmentVariable("TRACING_ENABLED", "false");
        try
        {
            var options = ObservabilityOptions.FromEnvironment();
            Assert.False(options.TracingEnabled);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TRACING_ENABLED", null);
        }
    }
}
