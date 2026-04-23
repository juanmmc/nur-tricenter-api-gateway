using System.Text;
using ApiGateway.Discovery;
using Xunit;

namespace ApiGateway.Tests.Discovery;

public class ConsulDiscoveryServiceTests
{
    [Fact]
    public void ParseInstances_Extracts_Healthy_Services_With_Service_Address()
    {
        const string json = """
        [
          {
            "Node": { "Address": "10.0.0.5" },
            "Service": { "ID": "svc-1", "Service": "patients", "Address": "154.38.180.80", "Port": 8080 }
          },
          {
            "Node": { "Address": "10.0.0.6" },
            "Service": { "ID": "svc-2", "Service": "patients", "Address": "154.38.180.80", "Port": 8081 }
          }
        ]
        """;

        var instances = ConsulDiscoveryService.ParseInstances(ToStream(json), "patients");

        Assert.Equal(2, instances.Count);
        Assert.Equal("154.38.180.80", instances[0].Address);
        Assert.Equal(8080, instances[0].Port);
        Assert.Equal("svc-1", instances[0].ServiceId);
        Assert.Equal("http://154.38.180.80:8080", instances[0].ToHttpUrl());
    }

    [Fact]
    public void ParseInstances_Falls_Back_To_Node_Address_When_Service_Address_Missing()
    {
        const string json = """
        [
          {
            "Node": { "Address": "192.168.1.10" },
            "Service": { "ID": "svc-1", "Service": "patients", "Address": "", "Port": 8080 }
          }
        ]
        """;

        var instances = ConsulDiscoveryService.ParseInstances(ToStream(json), "patients");

        var instance = Assert.Single(instances);
        Assert.Equal("192.168.1.10", instance.Address);
    }

    [Fact]
    public void ParseInstances_Skips_Entries_Without_Port()
    {
        const string json = """
        [
          {
            "Node": { "Address": "10.0.0.1" },
            "Service": { "ID": "svc-1", "Service": "patients", "Address": "10.0.0.1" }
          }
        ]
        """;

        var instances = ConsulDiscoveryService.ParseInstances(ToStream(json), "patients");

        Assert.Empty(instances);
    }

    [Fact]
    public void ParseInstances_Returns_Empty_For_Empty_Array()
    {
        var instances = ConsulDiscoveryService.ParseInstances(ToStream("[]"), "anything");
        Assert.Empty(instances);
    }

    private static Stream ToStream(string json) => new MemoryStream(Encoding.UTF8.GetBytes(json));
}
