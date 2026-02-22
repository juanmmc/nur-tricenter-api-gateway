using ApiGateway.Configuration.Microservices;

namespace ApiGateway.Configuration;

public class MicroserviceRegistry
{
    private readonly Dictionary<string, MicroserviceConfig> _services = new();

    public MicroserviceRegistry()
    {
        Register(new KeycloakConfig());
        Register(new LogisticsConfig());
        //Register(new ProductionConfig());
        //Register(new ContractsConfig());
        //Register(new PatientConfig());
    }

    public void Register(MicroserviceConfig service)
    {
        _services[service.ClusterId] = service;
    }

    public IEnumerable<MicroserviceConfig> GetAll() => _services.Values;
    public MicroserviceConfig Get(string clusterId) => _services[clusterId];
    public bool Exists(string clusterId) => _services.ContainsKey(clusterId);
}