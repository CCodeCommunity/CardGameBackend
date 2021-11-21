using System;
using System.Security.Claims;
using Api.Enums;

namespace Api.Utilities;

public static class ClaimsPrincipalUtils
{
    public static string Id(this ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.PrimarySid);
    
    public static string Name(this ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.Name);
    
    public static string Email(this ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.Name);
    
    public static AccountRole Role(this ClaimsPrincipal principal) => Enum.Parse<AccountRole>(principal.FindFirstValue(ClaimTypes.Role));

    public static AuthorizationScope Scope(this ClaimsPrincipal principal) => Enum.Parse<AuthorizationScope>(principal.FindFirstValue(ClaimTypes.Role));
}