using ApiGateway.Security;

namespace ApiGateway.Configuration.Microservices;

public class LogisticsConfig : MicroserviceConfig
{
    public override string Name => "Logistics";
    public override string ClusterId => "logistics";
    public override string BaseUrl => "http://localhost/";

    public override List<MicroserviceRoute> GetRoutes()
        {
            return new List<MicroserviceRoute>
            {
                // Driver Routes
                new MicroserviceRoute
                {
                    Name = "logistics-driver-create",
                    Path = "/logistics/api/Driver/createDriver",
                    Methods = new[] { "POST" },
                    AuthorizationPolicy = AuthPolicies.AdminOnly
                },
                new MicroserviceRoute
                {
                    Name = "logistics-driver-get",
                    Path = "/logistics/api/Driver/getDriver",
                    Methods = new[] { "GET" },
                    AuthorizationPolicy = AuthPolicies.LogisticsAccess
                },
                new MicroserviceRoute
                {
                    Name = "logistics-driver-get-drivers",
                    Path = "/logistics/api/Driver/getDrivers",
                    Methods = new[] { "GET" },
                    AuthorizationPolicy = AuthPolicies.AdminOnly
                },
                new MicroserviceRoute
                {
                    Name = "logistics-driver-update-location",
                    Path = "/logistics/api/Driver/updateDriverLocation",
                    Methods = new[] { "POST" },
                    AuthorizationPolicy = AuthPolicies.DriverOnly
                },

                // Package Routes
                new MicroserviceRoute
                {
                    Name = "logistics-package-create",
                    Path = "/logistics/api/Package/createPackage",
                    Methods = new[] { "POST" },
                    AuthorizationPolicy = AuthPolicies.AdminOnly
                },
                new MicroserviceRoute
                {
                    Name = "logistics-package-get",
                    Path = "/logistics/api/Package/getPackage",
                    Methods = new[] { "GET" },
                    AuthorizationPolicy = AuthPolicies.LogisticsAccess
                },
                new MicroserviceRoute
                {
                    Name = "logistics-package-get-by-driver",
                    Path = "/logistics/api/Package/getPackageByDriverAndDeliveryDate",
                    Methods = new[] { "GET" },
                    AuthorizationPolicy = AuthPolicies.LogisticsAccess
                },
                new MicroserviceRoute
                {
                    Name = "logistics-package-set-delivery-order",
                    Path = "/logistics/api/Package/setDeliveryOrder",
                    Methods = new[] { "POST" },
                    AuthorizationPolicy = AuthPolicies.DriverOnly
                },

                // Catch-all routes
                new MicroserviceRoute
                {
                    Name = "logistics-driver-all",
                    Path = "/logistics/api/Driver/{**catch-all}",
                    Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
                    AuthorizationPolicy = AuthPolicies.AdminOnly
                },
                new MicroserviceRoute
                {
                    Name = "logistics-general",
                    Path = "/logistics/api/{**catch-all}",
                    Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
                    AuthorizationPolicy = AuthPolicies.Authenticated
                }
            };
        }
}