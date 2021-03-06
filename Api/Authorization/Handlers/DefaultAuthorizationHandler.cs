using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Authorization.Requirements;
using Api.Services;
using Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Api.Authorization.Handlers;

public class DefaultAuthorizationHandler : AuthorizationHandler<DefaultAuthorization>
{
    private readonly AccessTokenTrackingService trackingService;
    private readonly IHttpContextAccessor httpContextAccessor;

    public DefaultAuthorizationHandler(AccessTokenTrackingService trackingService, IHttpContextAccessor httpContextAccessor)
    {
        this.trackingService = trackingService;
        this.httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DefaultAuthorization requirement)
    {
        var authHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString()!;
        if (authHeader.Length != 0)
        {
            var token = authHeader.Split(" ").Last();
            var claims = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var issuedAt = claims.ValidFrom;
            var accountId = claims.Claims.First(it => it.Type == ClaimTypes.PrimarySid).Value;
            var blackListed = await trackingService.IsTokenBlackListedAsync(accountId, issuedAt);

            if (blackListed == false)
            {
                context.Succeed(requirement);
                return;
            }
        }

        context.Fail();
    }
}