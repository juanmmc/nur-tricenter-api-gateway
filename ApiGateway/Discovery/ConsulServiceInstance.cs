namespace ApiGateway.Discovery;

public sealed record ConsulServiceInstance(string ServiceId, string Address, int Port)
{
    public string ToHttpUrl() => $"http://{Address}:{Port}";
}
