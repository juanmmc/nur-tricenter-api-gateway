using ApiGateway.Discovery;
using Xunit;

namespace ApiGateway.Tests.Discovery;

public class ConsulServiceRegistrarTests
{
    [Fact]
    public void BuildDefinition_Produces_Consul_Service_Payload()
    {
        var options = new ConsulOptions
        {
            ServiceId = "nur-tricenter-api-gateway",
            ServiceName = "nur-tricenter-api-gateway",
            ServiceAddress = "apigateway",
            ServicePort = 8080,
            HealthCheckPath = "/health",
            HealthCheckInterval = "10s",
            HealthCheckTimeout = "3s",
            DeregisterCriticalAfter = "30s",
            Tags = new[] { "gateway", "yarp" }
        };

        var payload = ConsulServiceRegistrar.BuildDefinition(options);

        Assert.Equal("nur-tricenter-api-gateway", payload["ID"]);
        Assert.Equal("nur-tricenter-api-gateway", payload["Name"]);
        Assert.Equal("apigateway", payload["Address"]);
        Assert.Equal(8080, payload["Port"]);
        Assert.Equal(new[] { "gateway", "yarp" }, payload["Tags"]);

        var check = Assert.IsType<Dictionary<string, object>>(payload["Check"]);
        Assert.Equal("http://apigateway:8080/health", check["HTTP"]);
        Assert.Equal("GET", check["Method"]);
        Assert.Equal("10s", check["Interval"]);
        Assert.Equal("3s", check["Timeout"]);
        Assert.Equal("30s", check["DeregisterCriticalServiceAfter"]);
    }
}
