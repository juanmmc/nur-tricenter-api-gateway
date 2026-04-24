namespace ApiGateway.Configuration.Microservices;

public class KeycloakConfig : MicroserviceConfig
{
    private const string Realm = "group3realm";
    public override string Name => "Keycloak";
    public override string ClusterId => "keycloak";
    public override string BaseUrl => "http://154.38.180.80:8080";

    public override List<MicroserviceRoute> GetRoutes() => new()
    {
        Route("keycloak-token", "/auth/token", "POST", null, "/protocol/openid-connect/token"),
        Route("keycloak-logout", "/auth/logout", "POST", "Authenticated", "/protocol/openid-connect/logout"),
        Route("keycloak-userinfo", "/auth/userinfo", "GET", "Authenticated", "/protocol/openid-connect/userinfo"),
        Route("keycloak-introspect", "/auth/introspect", "POST", "AdminOnly", "/protocol/openid-connect/token/introspect"),
        Route("keycloak-revoke", "/auth/revoke", "POST", "Authenticated", "/protocol/openid-connect/revoke"),
        Route("keycloak-auth", "/auth/authorize", "GET", null, "/protocol/openid-connect/auth"),
        Route("keycloak-certs", "/auth/certs", "GET", null, "/protocol/openid-connect/certs")
    };

    private static MicroserviceRoute Route(string name, string path, string method, string? policy, string keycloakPath) =>
        new()
        {
            Name = name,
            Path = path,
            Methods = new[] { method },
            AuthorizationPolicy = policy,
            CustomTransforms = new() { { "PathPattern", $"/realms/{Realm}{keycloakPath}" } }
        };
}