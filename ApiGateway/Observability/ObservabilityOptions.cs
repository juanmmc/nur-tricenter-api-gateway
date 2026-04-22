namespace ApiGateway.Observability;

public class ObservabilityOptions
{
    public string ServiceName { get; set; } = "nur-tricenter-api-gateway";
    public string ServiceVersion { get; set; } = "1.0.0";
    public bool TracingEnabled { get; set; } = true;
    public string ZipkinEndpoint { get; set; } = "http://jaeger:9411/api/v2/spans";

    public static ObservabilityOptions FromEnvironment()
    {
        return new ObservabilityOptions
        {
            ServiceName = Environment.GetEnvironmentVariable("SERVICE_NAME")
                ?? "nur-tricenter-api-gateway",
            ServiceVersion = Environment.GetEnvironmentVariable("SERVICE_VERSION")
                ?? "1.0.0",
            TracingEnabled = !bool.TryParse(
                Environment.GetEnvironmentVariable("TRACING_ENABLED"),
                out var enabled) || enabled,
            ZipkinEndpoint = Environment.GetEnvironmentVariable("ZIPKIN_ENDPOINT")
                ?? "http://jaeger:9411/api/v2/spans",
        };
    }
}
