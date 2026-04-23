using System.Net.Http.Json;

namespace ApiGateway.Discovery;

public class ConsulServiceRegistrar : IHostedService
{
    private readonly ConsulOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ConsulServiceRegistrar> _logger;

    public ConsulServiceRegistrar(
        ConsulOptions options,
        IHttpClientFactory httpClientFactory,
        ILogger<ConsulServiceRegistrar> logger)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Consul registration disabled (CONSUL_ENABLED=false).");
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("consul");
            var payload = BuildDefinition(_options);
            var uri = new Uri(_options.BaseUri, "/v1/agent/service/register");

            using var response = await client.PutAsJsonAsync(uri, payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Consul registration non-success: {Status} {Body}",
                    response.StatusCode, body);
                return;
            }
            _logger.LogInformation(
                "Registered service {ServiceId} with Consul at {Consul}",
                _options.ServiceId, _options.BaseUri);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Consul registration failed (continuing without registration).");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled) return;

        try
        {
            var client = _httpClientFactory.CreateClient("consul");
            var uri = new Uri(_options.BaseUri, $"/v1/agent/service/deregister/{_options.ServiceId}");
            using var response = await client.PutAsync(uri, content: null, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Deregistered service {ServiceId} from Consul.", _options.ServiceId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Consul deregistration failed.");
        }
    }

    public static Dictionary<string, object> BuildDefinition(ConsulOptions options)
    {
        var healthUrl = $"http://{options.ServiceAddress}:{options.ServicePort}{options.HealthCheckPath}";
        return new Dictionary<string, object>
        {
            ["ID"] = options.ServiceId,
            ["Name"] = options.ServiceName,
            ["Address"] = options.ServiceAddress,
            ["Port"] = options.ServicePort,
            ["Tags"] = options.Tags,
            ["Check"] = new Dictionary<string, object>
            {
                ["HTTP"] = healthUrl,
                ["Method"] = "GET",
                ["Interval"] = options.HealthCheckInterval,
                ["Timeout"] = options.HealthCheckTimeout,
                ["DeregisterCriticalServiceAfter"] = options.DeregisterCriticalAfter,
            },
        };
    }
}
