using ApiGateway.Configuration;

namespace ApiGateway.Discovery;

public class ConsulRoutePoller : BackgroundService
{
    private readonly MicroserviceRegistry _registry;
    private readonly ConsulDiscoveryService _discovery;
    private readonly ConsulProxyConfigProvider _provider;
    private readonly ConsulOptions _options;
    private readonly ILogger<ConsulRoutePoller> _logger;

    public ConsulRoutePoller(
        MicroserviceRegistry registry,
        ConsulDiscoveryService discovery,
        ConsulProxyConfigProvider provider,
        ConsulOptions options,
        ILogger<ConsulRoutePoller> logger)
    {
        _registry = registry;
        _discovery = discovery;
        _provider = provider;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Consul polling disabled. Routing will use static BaseUrl fallbacks.");
            return;
        }

        var interval = ParseInterval(_options.HealthCheckInterval, TimeSpan.FromSeconds(15));
        _logger.LogInformation("Consul route poller started with interval {Interval}.", interval);

        await PollOnceAsync(stoppingToken);
        using var timer = new PeriodicTimer(interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PollOnceAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task PollOnceAsync(CancellationToken cancellationToken)
    {
        var discoverable = _registry.GetAll().Where(s => s.UseDiscovery).ToArray();
        if (discoverable.Length == 0) return;

        var snapshot = new Dictionary<string, IReadOnlyList<ConsulServiceInstance>>(StringComparer.Ordinal);
        foreach (var service in discoverable)
        {
            var instances = await _discovery.GetHealthyInstancesAsync(service.ConsulServiceName, cancellationToken);
            snapshot[service.ConsulServiceName] = instances;
        }
        _provider.Update(snapshot);
    }

    public static TimeSpan ParseInterval(string raw, TimeSpan fallback)
    {
        if (string.IsNullOrWhiteSpace(raw)) return fallback;
        var trimmed = raw.Trim();
        var unit = trimmed[^1];
        var numericPart = trimmed[..^1];
        if (!double.TryParse(numericPart, System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            if (TimeSpan.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            return fallback;
        }
        return unit switch
        {
            's' or 'S' => TimeSpan.FromSeconds(value),
            'm' or 'M' => TimeSpan.FromMinutes(value),
            'h' or 'H' => TimeSpan.FromHours(value),
            _ => fallback,
        };
    }
}
