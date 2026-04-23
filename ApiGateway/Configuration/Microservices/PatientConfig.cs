using ApiGateway.Security;

namespace ApiGateway.Configuration.Microservices;

public class PatientConfig : MicroserviceConfig
{
    public override string Name => "patients";
    public override string ClusterId => "patients";
    public override string BaseUrl => Environment.GetEnvironmentVariable("PATIENTS_URL")
        ?? "http://154.38.180.80:8080";
    public override bool UseDiscovery => true;
    public override string ConsulServiceName => "nur-tricenter-patients";

    public override List<MicroserviceRoute> GetRoutes() => new()
    {
        new MicroserviceRoute
        {
            Name = "patients-login",
            Path = "/auth/patient/login",
            Methods = new[] { "POST" },
            AuthorizationPolicy = null,
            CustomTransforms = new() { { "PathPattern", "/api/login" } }
        },
        new MicroserviceRoute
        {
            Name = "patients-refresh",
            Path = "/auth/patient/refresh",
            Methods = new[] { "POST" },
            AuthorizationPolicy = null,
            CustomTransforms = new() { { "PathPattern", "/api/refresh" } }
        },
        new MicroserviceRoute
        {
            Name = "patients-root",
            Path = "/patients",
            Methods = new[] { "GET", "POST" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/api/patient" } }
        },
        new MicroserviceRoute
        {
            Name = "patients-all",
            Path = "/patients/{**catch-all}",
            Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/api/patient/{**catch-all}" } }
        }
    };
}
