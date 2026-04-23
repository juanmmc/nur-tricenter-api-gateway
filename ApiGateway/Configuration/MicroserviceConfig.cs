namespace ApiGateway.Configuration;

public abstract class MicroserviceConfig
{
    public abstract string Name { get; }
    public abstract string ClusterId { get; }
    public abstract string BaseUrl { get; }
    public abstract List<MicroserviceRoute> GetRoutes();

    public virtual bool UseDiscovery => false;
    public virtual string ConsulServiceName => ClusterId;
}
