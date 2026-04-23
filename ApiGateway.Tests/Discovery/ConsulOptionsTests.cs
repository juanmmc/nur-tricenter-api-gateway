using ApiGateway.Discovery;
using Xunit;

namespace ApiGateway.Tests.Discovery;

public class ConsulOptionsTests
{
    [Fact]
    public void BaseUri_Composes_From_Scheme_Host_Port()
    {
        var options = new ConsulOptions
        {
            Scheme = "http",
            Host = "consul",
            Port = 8500
        };

        Assert.Equal(new Uri("http://consul:8500"), options.BaseUri);
    }

    [Fact]
    public void DefaultTags_Contain_Gateway_And_Yarp()
    {
        var options = new ConsulOptions();

        Assert.Contains("gateway", options.Tags);
        Assert.Contains("yarp", options.Tags);
    }

    [Fact]
    public void FromEnvironment_Uses_Defaults_When_Empty()
    {
        Environment.SetEnvironmentVariable("CONSUL_HOST", null);
        Environment.SetEnvironmentVariable("CONSUL_PORT", null);
        Environment.SetEnvironmentVariable("SERVICE_ID", null);

        var options = ConsulOptions.FromEnvironment();

        Assert.Equal("consul", options.Host);
        Assert.Equal(8500, options.Port);
        Assert.Equal("nur-tricenter-api-gateway", options.ServiceId);
    }

    [Fact]
    public void FromEnvironment_Respects_Overrides()
    {
        Environment.SetEnvironmentVariable("CONSUL_HOST", "my-consul");
        Environment.SetEnvironmentVariable("CONSUL_PORT", "9500");
        Environment.SetEnvironmentVariable("SERVICE_ID", "my-service");

        try
        {
            var options = ConsulOptions.FromEnvironment();

            Assert.Equal("my-consul", options.Host);
            Assert.Equal(9500, options.Port);
            Assert.Equal("my-service", options.ServiceId);
        }
        finally
        {
            Environment.SetEnvironmentVariable("CONSUL_HOST", null);
            Environment.SetEnvironmentVariable("CONSUL_PORT", null);
            Environment.SetEnvironmentVariable("SERVICE_ID", null);
        }
    }
}
