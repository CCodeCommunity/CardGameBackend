using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Enums;
using Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services;

public class AuthTokenService
{
    private readonly string jwtKey;
    private readonly string jwtIssuer;
    private readonly int jwtAccessTokenExpiryInMinutes;
    private readonly int jwtRefreshTokenExpiryInMonths;
        
    public AuthTokenService(IConfiguration config)
    {
        jwtKey = config["Jwt:Key"];
        jwtIssuer = config["Jwt:Issuer"];
        jwtAccessTokenExpiryInMinutes = int.Parse(config["Jwt:AccessTokenExpiryInMinutes"]);
        jwtRefreshTokenExpiryInMonths = int.Parse(config["Jwt:RefreshTokenExpiryInMonths"]);
    }
    
    public string? GenerateRefreshToken(Account account)
    {
        var claims = new[] 
        {
            new Claim(ClaimTypes.PrimarySid, account.Id),
            new Claim(ClaimTypes.Name, account.Name),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role.ToString()),
            new Claim("scope", AuthorizationScope.Refresh.ToString())
        };
        
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMonths(jwtRefreshTokenExpiryInMonths);

        return generateToken(claims, issuedAt, expiresAt);
    }
    
    public string? GenerateAccessToken(Account account)
    {
        var claims = new[] 
        {
            new Claim(ClaimTypes.PrimarySid, account.Id),
            new Claim(ClaimTypes.Name, account.Name),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role.ToString()),
            new Claim("scope", AuthorizationScope.Access.ToString())
        };
        
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(jwtAccessTokenExpiryInMinutes);

        return generateToken(claims, issuedAt, expiresAt);
    }

    private string? generateToken(IEnumerable<Claim> claims, DateTime issuedAt, DateTime expiresAt)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(
            new JwtHeader(signingCredentials: credentials),
            new JwtPayload(issuer: jwtIssuer, audience: jwtIssuer, claims: claims, 
                expires: expiresAt, issuedAt: issuedAt, notBefore: null));

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);  
    }
        
    public ClaimsPrincipal? IsAccessTokenValid(string token)
    {
        var mySecret = Encoding.UTF8.GetBytes(jwtKey);           
        var mySecurityKey = new SymmetricSecurityKey(mySecret);
        var tokenHandler = new JwtSecurityTokenHandler(); 
            
        try 
        {
            return tokenHandler.ValidateToken(token, 
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtIssuer, 
                    IssuerSigningKey = mySecurityKey
                }, out _);
        }
        catch
        {
            return null;
        }
    }
}