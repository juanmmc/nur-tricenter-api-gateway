using Microsoft.AspNetCore.Authorization;

namespace ApiGateway.Security;
public static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string DriverOnly = "DriverOnly";
    public const string LogisticsAccess = "LogisticsAccess";
    public const string Authenticated = "Authenticated";

    public static void Register(AuthorizationOptions options)
    {
        options.AddPolicy(AdminOnly, p => p.RequireRole("admin"));
        options.AddPolicy(DriverOnly, p => p.RequireRole("driver"));
        options.AddPolicy(LogisticsAccess, p => p.RequireRole("driver", "admin"));
        options.AddPolicy(Authenticated, p => p.RequireAuthenticatedUser());
    }
}