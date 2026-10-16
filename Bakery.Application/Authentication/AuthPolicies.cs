namespace Bakery.Application.Authentication;

public static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string CustomerOnly = "CustomerOnly";
    public const string CustomerOrAdmin = "CustomerOrAdmin";
}
