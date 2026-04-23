namespace ApiGateway.Discovery;

public class ConsulOptions
{
    public bool Enabled { get; set; } = true;
    public string Host { get; set; } = "consul";
    public int Port { get; set; } = 8500;
    public string Scheme { get; set; } = "http";

    public string ServiceId { get; set; } = "nur-tricenter-api-gateway";
    public string ServiceName { get; set; } = "nur-tricenter-api-gateway";
    public string ServiceAddress { get; set; } = "apigateway";
    public int ServicePort { get; set; } = 8080;
    public string HealthCheckPath { get; set; } = "/health";
    public string HealthCheckInterval { get; set; } = "15s";
    public string HealthCheckTimeout { get; set; } = "5s";
    public string DeregisterCriticalAfter { get; set; } = "1m";
    public string[] Tags { get; set; } = new[] { "gateway", "yarp" };

    public Uri BaseUri => new($"{Scheme}://{Host}:{Port}");

    public static ConsulOptions FromEnvironment()
    {
        return new ConsulOptions
        {
            Enabled = !bool.TryParse(
                Environment.GetEnvironmentVariable("CONSUL_ENABLED"),
                out var enabled) || enabled,
            Host = Environment.GetEnvironmentVariable("CONSUL_HOST") ?? "consul",
            Port = int.TryParse(Environment.GetEnvironmentVariable("CONSUL_PORT"), out var p) ? p : 8500,
            Scheme = Environment.GetEnvironmentVariable("CONSUL_SCHEME") ?? "http",
            ServiceId = Environment.GetEnvironmentVariable("SERVICE_ID")
                ?? "nur-tricenter-api-gateway",
            ServiceName = Environment.GetEnvironmentVariable("SERVICE_NAME")
                ?? "nur-tricenter-api-gateway",
            ServiceAddress = Environment.GetEnvironmentVariable("SERVICE_ADDRESS") ?? "apigateway",
            ServicePort = int.TryParse(Environment.GetEnvironmentVariable("SERVICE_PORT"), out var sp) ? sp : 8080,
            HealthCheckPath = Environment.GetEnvironmentVariable("HEALTH_CHECK_PATH") ?? "/health",
            HealthCheckInterval = Environment.GetEnvironmentVariable("HEALTH_CHECK_INTERVAL") ?? "15s",
            HealthCheckTimeout = Environment.GetEnvironmentVariable("HEALTH_CHECK_TIMEOUT") ?? "5s",
            DeregisterCriticalAfter = Environment.GetEnvironmentVariable("CONSUL_DEREGISTER_AFTER") ?? "1m",
        };
    }
}
