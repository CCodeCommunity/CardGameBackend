using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services;

public class AuthTokenService
{
    private readonly string jwtKey;
        
    private readonly string jwtIssuer;

    private readonly int jwtAccessTokenExpiry;
        
    public AuthTokenService(IConfiguration config)
    {
        jwtKey = config["Jwt:Key"];
        jwtIssuer = config["Jwt:Issuer"];
        jwtAccessTokenExpiry = int.Parse(config["Jwt:AccessTokenExpiryInMinutes"]);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public string? GenerateAccessToken(Account account)
    {
        var claims = new[] 
        {
            new Claim(ClaimTypes.PrimarySid, account.Id),
            new Claim(ClaimTypes.Name, account.Name),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
        };

        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(jwtAccessTokenExpiry);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(
            new JwtHeader(signingCredentials: credentials),
            new JwtPayload(issuer: jwtIssuer, audience: jwtIssuer, claims: claims, expires: expiresAt, issuedAt: issuedAt, notBefore: null));

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
                    ValidateIssuer = true, 
                    ValidateAudience = true,    
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