using ApiGateway.Configuration;
using ApiGateway.Discovery;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ApiGateway.Tests.Discovery;

public class ConsulProxyConfigProviderTests
{
    [Fact]
    public void GetConfig_Initial_Snapshot_Falls_Back_To_BaseUrl_For_Discoverable_Services()
    {
        var provider = NewProvider();

        var config = provider.GetConfig();

        var patients = config.Clusters.Single(c => c.ClusterId == "patients");
        var destination = Assert.Single(patients.Destinations!);
        Assert.Equal("fallback", destination.Key);
        Assert.NotNull(destination.Value.Address);
    }

    [Fact]
    public void GetConfig_Static_Cluster_Always_Uses_BaseUrl()
    {
        var provider = NewProvider();

        var keycloak = provider.GetConfig().Clusters.Single(c => c.ClusterId == "keycloak");
        var destination = Assert.Single(keycloak.Destinations!);
        Assert.Equal("fallback", destination.Key);
        Assert.StartsWith("http://", destination.Value.Address);
    }

    [Fact]
    public void Update_Replaces_Destinations_With_Live_Consul_Instances()
    {
        var provider = NewProvider();
        var snapshot = new Dictionary<string, IReadOnlyList<ConsulServiceInstance>>(StringComparer.Ordinal)
        {
            ["nur-tricenter-patients"] = new[]
            {
                new ConsulServiceInstance("svc-a", "10.1.1.1", 8080),
                new ConsulServiceInstance("svc-b", "10.1.1.2", 8080),
            },
        };

        provider.Update(snapshot);

        var patients = provider.GetConfig().Clusters.Single(c => c.ClusterId == "patients");
        Assert.Equal(2, patients.Destinations!.Count);
        Assert.Contains(patients.Destinations, d => d.Value.Address == "http://10.1.1.1:8080");
        Assert.Contains(patients.Destinations, d => d.Value.Address == "http://10.1.1.2:8080");
    }

    [Fact]
    public void Update_With_Empty_Instances_Reverts_To_Fallback()
    {
        var provider = NewProvider();
        provider.Update(new Dictionary<string, IReadOnlyList<ConsulServiceInstance>>(StringComparer.Ordinal)
        {
            ["nur-tricenter-patients"] = Array.Empty<ConsulServiceInstance>(),
        });

        var patients = provider.GetConfig().Clusters.Single(c => c.ClusterId == "patients");
        var destination = Assert.Single(patients.Destinations!);
        Assert.Equal("fallback", destination.Key);
    }

    [Fact]
    public void Update_Triggers_ChangeToken()
    {
        var provider = NewProvider();
        var initial = provider.GetConfig();
        Assert.False(initial.ChangeToken.HasChanged);

        provider.Update(new Dictionary<string, IReadOnlyList<ConsulServiceInstance>>(StringComparer.Ordinal));

        Assert.True(initial.ChangeToken.HasChanged);
    }

    [Fact]
    public void GetConfig_Builds_Routes_With_Cluster_Ids_And_Methods()
    {
        var provider = NewProvider();

        var routes = provider.GetConfig().Routes;

        Assert.NotEmpty(routes);
        Assert.All(routes, r =>
        {
            Assert.False(string.IsNullOrWhiteSpace(r.RouteId));
            Assert.False(string.IsNullOrWhiteSpace(r.ClusterId));
            Assert.NotNull(r.Match);
        });
    }

    private static ConsulProxyConfigProvider NewProvider()
        => new(new MicroserviceRegistry(), NullLogger<ConsulProxyConfigProvider>.Instance);
}
