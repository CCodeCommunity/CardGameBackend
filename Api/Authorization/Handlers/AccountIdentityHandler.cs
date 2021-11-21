using System.Threading.Tasks;
using Api.Authorization.Requirements;
using Api.Enums;
using Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Api.Authorization.Handlers;

public class AccountIdentityHandler : AuthorizationHandler<AccountIdentityAuthorization>
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public AccountIdentityHandler(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }
    
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccountIdentityAuthorization requirement)
    {
        var accountId = context.User.Id();

        var requestAccountId = httpContextAccessor.HttpContext?.GetRouteData()?.Values["accountId"]?.ToString();

        if (requestAccountId != null)
        {
            var valid = requestAccountId == accountId && context.User.Role() != AccountRole.Admin;

            if (valid)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        context.Fail();
        return Task.CompletedTask;
    }
}