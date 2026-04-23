using System.Text.Json;

namespace ApiGateway.Discovery;

public class ConsulDiscoveryService
{
    private readonly ConsulOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ConsulDiscoveryService> _logger;

    public ConsulDiscoveryService(
        ConsulOptions options,
        IHttpClientFactory httpClientFactory,
        ILogger<ConsulDiscoveryService> logger)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ConsulServiceInstance>> GetHealthyInstancesAsync(
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            return Array.Empty<ConsulServiceInstance>();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("consul");
            var uri = new Uri(_options.BaseUri, $"/v1/health/service/{Uri.EscapeDataString(serviceName)}?passing=true");

            using var response = await client.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Consul query for {Service} returned {Status}.",
                    serviceName, response.StatusCode);
                return Array.Empty<ConsulServiceInstance>();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return ParseInstances(stream, serviceName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Consul discovery failed for {Service}.", serviceName);
            return Array.Empty<ConsulServiceInstance>();
        }
    }

    public static IReadOnlyList<ConsulServiceInstance> ParseInstances(Stream stream, string serviceName)
    {
        var entries = JsonSerializer.Deserialize<List<JsonElement>>(stream)
            ?? new List<JsonElement>();
        var result = new List<ConsulServiceInstance>(entries.Count);
        foreach (var entry in entries)
        {
            if (!entry.TryGetProperty("Service", out var service)) continue;

            var serviceId = service.TryGetProperty("ID", out var idEl)
                ? idEl.GetString() ?? serviceName
                : serviceName;
            var address = ResolveAddress(entry, service);
            var port = service.TryGetProperty("Port", out var portEl) && portEl.ValueKind == JsonValueKind.Number
                ? portEl.GetInt32()
                : 0;

            if (string.IsNullOrWhiteSpace(address) || port <= 0) continue;
            result.Add(new ConsulServiceInstance(serviceId, address, port));
        }
        return result;
    }

    private static string ResolveAddress(JsonElement entry, JsonElement service)
    {
        if (service.TryGetProperty("Address", out var svcAddr)
            && svcAddr.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(svcAddr.GetString()))
        {
            return svcAddr.GetString()!;
        }
        if (entry.TryGetProperty("Node", out var node)
            && node.TryGetProperty("Address", out var nodeAddr)
            && nodeAddr.ValueKind == JsonValueKind.String)
        {
            return nodeAddr.GetString() ?? string.Empty;
        }
        return string.Empty;
    }
}
