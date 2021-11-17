using Api.Enums;

namespace Api.Authorization;

public class AuthorizationRoles
{
    public const string User = nameof(AccountRole.User);
    public const string Admin = nameof(AccountRole.Admin);
}