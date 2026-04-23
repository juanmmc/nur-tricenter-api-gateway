using ApiGateway.Configuration;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace ApiGateway.Discovery;

public class ConsulProxyConfigProvider : IProxyConfigProvider
{
    private readonly MicroserviceRegistry _registry;
    private readonly ILogger<ConsulProxyConfigProvider> _logger;
    private volatile ConsulProxyConfig _config;
    private CancellationTokenSource _changeTokenSource;
    private readonly object _lock = new();

    public ConsulProxyConfigProvider(
        MicroserviceRegistry registry,
        ILogger<ConsulProxyConfigProvider> logger)
    {
        _registry = registry;
        _logger = logger;
        _changeTokenSource = new CancellationTokenSource();
        _config = BuildConfig(EmptyInstances(), _changeTokenSource);
    }

    public IProxyConfig GetConfig() => _config;

    public void Update(IReadOnlyDictionary<string, IReadOnlyList<ConsulServiceInstance>> instances)
    {
        CancellationTokenSource oldCts;
        lock (_lock)
        {
            oldCts = _changeTokenSource;
            _changeTokenSource = new CancellationTokenSource();
            _config = BuildConfig(instances, _changeTokenSource);
        }
        try
        {
            oldCts.Cancel();
        }
        finally
        {
            oldCts.Dispose();
        }
        _logger.LogDebug("Proxy config refreshed: {ClusterCount} clusters.", _config.Clusters.Count);
    }

    private ConsulProxyConfig BuildConfig(
        IReadOnlyDictionary<string, IReadOnlyList<ConsulServiceInstance>> instances,
        CancellationTokenSource cts)
    {
        var routes = new List<RouteConfig>();
        var clusters = new List<ClusterConfig>();

        foreach (var service in _registry.GetAll())
        {
            clusters.Add(BuildCluster(service, instances));
            foreach (var route in service.GetRoutes())
            {
                routes.Add(BuildRoute(service, route));
            }
        }

        return new ConsulProxyConfig(routes, clusters, new CancellationChangeToken(cts.Token));
    }

    private ClusterConfig BuildCluster(
        MicroserviceConfig service,
        IReadOnlyDictionary<string, IReadOnlyList<ConsulServiceInstance>> instances)
    {
        var destinations = new Dictionary<string, DestinationConfig>(StringComparer.Ordinal);

        if (service.UseDiscovery
            && instances.TryGetValue(service.ConsulServiceName, out var live)
            && live.Count > 0)
        {
            foreach (var instance in live)
            {
                destinations[$"consul-{instance.ServiceId}"] = new DestinationConfig
                {
                    Address = instance.ToHttpUrl(),
                };
            }
        }
        else
        {
            destinations["fallback"] = new DestinationConfig
            {
                Address = service.BaseUrl,
            };
            if (service.UseDiscovery)
            {
                _logger.LogWarning(
                    "Cluster {Cluster}: no healthy Consul instances for {ConsulName}, using BaseUrl fallback {Url}.",
                    service.ClusterId, service.ConsulServiceName, service.BaseUrl);
            }
        }

        return new ClusterConfig
        {
            ClusterId = service.ClusterId,
            LoadBalancingPolicy = "PowerOfTwoChoices",
            Destinations = destinations,
        };
    }

    private static RouteConfig BuildRoute(MicroserviceConfig service, MicroserviceRoute route)
    {
        var transforms = new List<IReadOnlyDictionary<string, string>>();
        if (route.CustomTransforms is { Count: > 0 })
        {
            transforms.Add(route.CustomTransforms);
        }
        else
        {
            transforms.Add(new Dictionary<string, string>
            {
                ["PathRemovePrefix"] = $"/{service.Name.ToLowerInvariant()}",
            });
        }

        return new RouteConfig
        {
            RouteId = route.Name,
            ClusterId = service.ClusterId,
            AuthorizationPolicy = route.AuthorizationPolicy,
            Match = new RouteMatch
            {
                Path = route.Path,
                Methods = route.Methods,
            },
            Transforms = transforms,
        };
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<ConsulServiceInstance>> EmptyInstances()
        => new Dictionary<string, IReadOnlyList<ConsulServiceInstance>>(StringComparer.Ordinal);
}
