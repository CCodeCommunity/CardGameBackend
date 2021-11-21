namespace Api.Authorization;

public static class AuthorizationPolicies
{
    public const string DefaultPolicy = "DefaultPolicy";
    public const string RequireMatchingAccountId = "AccountIdentityPolicy";
    public const string RequireAdminOnly = "AdminOnly";
}