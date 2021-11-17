namespace Api.Controllers;

using System;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Authorization;
using Api.Dtos;
using Api.Enums;
using Api.Models;
using Api.Services;
using Api.Utilities;
using Ardalis.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

[ValidateModel]
[ApiController, Route("api/[controller]")]
[Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
public class AccountsController : Controller
{
    private readonly DatabaseContext db;
    private readonly AuthTokenService tokenService;
    private readonly IHttpContextAccessor httpContextAccessor;

    public AccountsController(DatabaseContext db, IHttpContextAccessor httpContextAccessor, AuthTokenService tokenService)
    {
        this.db = db;
        this.tokenService = tokenService;
        this.httpContextAccessor = httpContextAccessor;
    }

    [HttpPatch("access-token")]
    public async Task<ActionResult> GetAccessToken([FromBody] GetAccessToken.Request request)
    {
        var claimsAccountId = tokenService
            .IsAccessTokenValid(request.AccessToken)?.Claims
            .First(it => it.Type == ClaimTypes.PrimarySid).Value;
            
        if (claimsAccountId == null) 
        {
            // Check if refresh token is valid and delete it all such tokens for the user
            return BadRequest();
        }

        var loginInstance = await db.LoginInstance
            .Include(it => it.Account)
            .FirstOrDefaultAsync(it => it.Token == request.RefreshToken);
            
        if (loginInstance == null)
        {
            // Refresh token does not match access token, access token was compromised
            await db.LoginInstance.Where(it => it.AccountId == claimsAccountId).DeleteAsync();
            await db.SaveChangesAsync();
            return BadRequest();
        }

        if (loginInstance.AccountId != claimsAccountId)
        {
            // Refresh token does not match access token, tokens were compromised
            await db.LoginInstance.Where(it => it.AccountId == loginInstance.AccountId).DeleteAsync();
            await db.LoginInstance.Where(it => it.AccountId == claimsAccountId).DeleteAsync();
            await db.SaveChangesAsync();
            return BadRequest();
        }
            
        var newAccessToken = tokenService.GenerateAccessToken(loginInstance.Account);
        if (newAccessToken == null) return BadRequest();
            
        var newRefreshToken = tokenService.GenerateRefreshToken();

        return Ok(new GetAccessToken.Response(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshToken
        ));
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] Login.Request request)
    {
        var account = await db.Accounts.Where(it => it.Email == request.Email).FirstOrDefaultAsync();
        if (account == null) return BadRequest();

        var passwordValid = await Argon2Utils.VerifyHashAsync(request.Password, account.Password);
        if (!passwordValid) return BadRequest();
            
        var accessToken = tokenService.GenerateAccessToken(account);
        if (accessToken == null) return BadRequest();

        var refreshToken = tokenService.GenerateRefreshToken();
        
        await db.LoginInstance.AddAsync(new LoginInstance
        {
            Token = refreshToken,
            Device = request.Device,
            DeviceAgent = request.DeviceAgent,
            DeviceOS = request.DeviceOS,
            Ip = httpContextAccessor.HttpContext!.GetRequestIP(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMonths(10),
            AccountId = account.Id
        });

        await db.SaveChangesAsync();

        return Ok(new Login.Response(
            RefreshToken: refreshToken,
            AccessToken: accessToken
        ));
    }
    
    [HttpPost]
    public async Task<ActionResult> CreateAccount([FromBody] CreateAccount.Request request)
    {
        var newAccount = new Account
        {
            Id = await Nanoid.Nanoid.GenerateAsync(),
            Name = request.Name,
            Email = request.Email,
            Password = await Argon2Utils.HashPassword(request.Password),
            Role = AccountRole.User,
            State = AccountState.PendingApproval
        };

        await db.Accounts.AddAsync(newAccount);

        await db.SaveChangesAsync();

        return Ok();
    }
    
    [HttpPatch("{accountId}/state")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> ChangeAccountState(string accountId, [FromBody] ChangeAccountState.Request request)
    {
        var account = await db.Accounts.FindAsync(accountId);
        if (account == null) return BadRequest();

        if (account.State != request.NewState)
        {
            if (request.NewState == AccountState.PendingApproval)
            {
                return BadRequest();
            }

            if (request.NewState == AccountState.Suspended)
            {
                // Todo: Email user that the account is now suspended
            }
            
            account.State = request.NewState;
        }

        await db.SaveChangesAsync();
        
        return Ok();
    }

    [HttpGet]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> GetAccounts([FromQuery] int page, [FromQuery] int pageSize)
    {
        var result = await db.Accounts.GetPagedAsync(page, pageSize);

        return Ok(result);
    }
}