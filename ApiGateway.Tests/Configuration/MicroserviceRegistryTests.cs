using ApiGateway.Configuration;
using Xunit;

namespace ApiGateway.Tests.Configuration;

public class MicroserviceRegistryTests
{
    [Fact]
    public void Registry_Contains_Known_Clusters()
    {
        var registry = new MicroserviceRegistry();

        Assert.True(registry.Exists("keycloak"));
        Assert.True(registry.Exists("patients"));
        Assert.NotEmpty(registry.GetAll());
    }

    [Fact]
    public void GetAll_Returns_All_Registered_Services()
    {
        var registry = new MicroserviceRegistry();

        var services = registry.GetAll().ToList();

        Assert.True(services.Count >= 7,
            $"Expected at least 7 services, found {services.Count}.");
    }
}
