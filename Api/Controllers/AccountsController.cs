using System;
using System.ComponentModel.DataAnnotations;
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

namespace Api.Controllers;

[ValidateModel]
[ApiController, Route("api/[controller]")]
[Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
public class AccountsController : Controller
{
    private readonly DatabaseContext db;
    private readonly AuthTokenService tokenService;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly AccessTokenBlackListService blackListService;

    public AccountsController(
        DatabaseContext db, 
        IHttpContextAccessor httpContextAccessor, 
        AuthTokenService tokenService,
        AccessTokenBlackListService blackListService)
    {
        this.db = db;
        this.tokenService = tokenService;
        this.blackListService = blackListService;
        this.httpContextAccessor = httpContextAccessor;
    }

    [HttpPatch("access-token")]
    public async Task<ActionResult<GetAccessToken.Request>> GetAccessToken([FromBody] GetAccessToken.Request request)
    {
        var loginInstance = await db.LoginInstance
            .Include(it => it.Account)
            .FirstOrDefaultAsync(it => it.Token == request.RefreshToken);
        
        if (loginInstance == null)
            return BadRequest();

        if (loginInstance.Valid == false)
        {
            // Refresh token does not match access token, tokens were compromised
            await db.LoginInstance
                .Where(it => it.AccountId == loginInstance.AccountId && it.Valid == true)
                .UpdateAsync(it => new LoginInstance {Valid = false});

            await blackListService.BlackListAccessTokensForUserAsync(loginInstance.AccountId);
            
            await db.SaveChangesAsync();
            
            return BadRequest();
        }
            
        var newAccessToken = tokenService.GenerateAccessToken(loginInstance.Account);
        if (newAccessToken == null) 
            return BadRequest();
            
        var newRefreshToken = tokenService.GenerateRefreshToken(loginInstance.Account);
        if (newRefreshToken == null) 
            return BadRequest();

        var blackListed = await blackListService.IsTokenBlackListedAsync(loginInstance.AccountId, loginInstance.CreatedAt);
        if (blackListed)
            return BadRequest();

        return Ok(new GetAccessToken.Response(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshToken
        ));
    }

    [HttpDelete("login-instance")]
    public async Task<ActionResult> Logout([FromBody] Logout.Request request)
    {
        var login = await db.LoginInstance.FindAsync(request.RefreshToken);
        if (login == null) return UnprocessableEntity();

        login.Valid = false;

        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<Login.Response>> Login([FromBody] Login.Request request)
    {
        var account = await db.Accounts.Where(it => it.Email == request.Email).FirstOrDefaultAsync();
        if (account == null) 
            return BadRequest();

        var passwordValid = await Argon2Utils.VerifyHashAsync(request.Password, account.Password);
        if (!passwordValid) 
            return BadRequest();
            
        var accessToken = tokenService.GenerateAccessToken(account);
        if (accessToken == null) 
            return BadRequest();

        var refreshToken = tokenService.GenerateRefreshToken(account);
        if (refreshToken == null) 
            return BadRequest();
        
        await db.LoginInstance.AddAsync(new LoginInstance
        {
            Token = refreshToken,
            Device = request.Device,
            DeviceAgent = request.DeviceAgent,
            DeviceOS = request.DeviceOS,
            Ip = httpContextAccessor.HttpContext!.GetRequestIP(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMonths(10),
            AccountId = account.Id,
            Valid = true,
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
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOnly)]
    public async Task<ActionResult> ChangeAccountState(string accountId, [FromBody] ChangeAccountState.Request request)
    {
        var account = await db.Accounts.FindAsync(accountId);
        if (account == null) return BadRequest();

        if (account.State != request.NewState)
        {
            if (request.NewState == AccountState.PendingApproval)
                return BadRequest();

            if (request.NewState == AccountState.Suspended)
            {
                // Todo: Email user that the account is now suspended
                await blackListService.BlackListAccessTokensForUserAsync(accountId);
            }
            
            account.State = request.NewState;
        }

        await db.SaveChangesAsync();
        
        return Ok();
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminOnly)]
    public async Task<ActionResult<PagedResult<Account>>> GetAccounts(
        [FromQuery, Range(1, int.MaxValue)] int page, 
        [FromQuery, Range(1, int.MaxValue)] int pageSize)
    {
        var result = await db.Accounts.GetPagedAsync(page, pageSize);

        return Ok(result);
    }

    [HttpGet("profiles")]
    [Authorize]
    public async Task<ActionResult<PagedResult<Profile>>> GetProfiles(
        [FromQuery, Range(1, int.MaxValue)] int page, 
        [FromQuery, Range(1, int.MaxValue)] int pageSize)
    {
        var result = await db.Accounts
            .Select(it => new Profile(it.Id, it.Name, it.ProfilePicture))
            .GetPagedAsync(page, pageSize);

        return Ok(result);
    }

    [HttpGet("{accountId}/profile")]
    [Authorize(Policy = AuthorizationPolicies.RequireMatchingAccountId)]
    public async Task<ActionResult<Profile>> GetProfile(string accountId)
    {
        var result = await db.Accounts
            .Select(it => new Profile(it.Id, it.Name, it.ProfilePicture))
            .FirstOrDefaultAsync(it => it.Id == accountId);

        if (result == null) 
            return UnprocessableEntity();

        return Ok(result);
    }
    
    [HttpPatch("{accountId}/profile")]
    [Authorize(Policy = AuthorizationPolicies.RequireMatchingAccountId)]
    public async Task<ActionResult> PatchProfile(string accountId, [FromBody] PatchProfile.Request request)
    {
        var result = await db.Accounts.FindAsync(accountId);

        if (result == null) 
            return UnprocessableEntity();

        result.Name = request.Name ?? result.Name;
        result.ProfilePicture = request.ProfilePicture ?? result.ProfilePicture;

        return Ok();
    }
}