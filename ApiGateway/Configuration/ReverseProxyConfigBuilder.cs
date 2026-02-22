using Microsoft.Extensions.Configuration;

namespace ApiGateway.Configuration;

public static class ReverseProxyConfigBuilder
{
    public static void ConfigureMicroserviceRoutes(
        IConfigurationBuilder config,
        MicroserviceRegistry registry)
    {
        var data = BuildReverseProxyConfig(registry);
        config.AddInMemoryCollection(data);
    }

    private static IDictionary<string, string?> BuildReverseProxyConfig(
        MicroserviceRegistry registry)
    {
        var data = new Dictionary<string, string?>();

        // --- Microservicios ---
        foreach (var service in registry.GetAll())
        {
            // Cluster
            data[$"ReverseProxy:Clusters:{service.ClusterId}:Destinations:destination1:Address"] = service.BaseUrl;

            foreach (var route in service.GetRoutes())
            {
                var baseKey = $"ReverseProxy:Routes:{route.Name}";

                data[$"{baseKey}:ClusterId"] = service.ClusterId;

                if (!string.IsNullOrWhiteSpace(route.AuthorizationPolicy))
                {
                    data[$"{baseKey}:AuthorizationPolicy"] = route.AuthorizationPolicy;
                }

                if (!string.IsNullOrWhiteSpace(route.Path))
                {
                    data[$"{baseKey}:Match:Path"] = route.Path;
                }

                if (route.Methods is not null)
                {
                    for (var i = 0; i < route.Methods.Length; i++)
                    {
                        data[$"{baseKey}:Match:Methods:{i}"] = route.Methods[i];
                    }
                }

                if (route.CustomTransforms?.Any() == true)
                {
                    var idx = 0;
                    foreach (var kvp in route.CustomTransforms)
                    {
                        data[$"{baseKey}:Transforms:{idx++}:{kvp.Key}"] = kvp.Value;
                    }
                }
                else
                {
                    data[$"{baseKey}:Transforms:0:PathRemovePrefix"] = $"/{service.Name.ToLower()}";
                }
            }
        }

        return data;
    }
}