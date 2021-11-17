using System.Security.Claims;
using System.Threading.Tasks;
using Api.Authorization.Requirements;
using Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization.Handlers;

public class DefaultAuthorizationHandler : AuthorizationHandler<DefaultAuthorization>
{
    private readonly AccountStateValidationService validationService;

    public DefaultAuthorizationHandler(AccountStateValidationService validationService)
    {
        this.validationService = validationService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DefaultAuthorization requirement)
    {
        var accountId = context.User.FindFirst(c => c.Type == ClaimTypes.PrimarySid)?.Value;

        if (accountId != null)
        {
            var valid = await validationService.ValidateAccountStateAsync(accountId);

            if (valid)
            {
                context.Succeed(requirement);
                return;
            }
        }
            
        context.Fail();
    }
}