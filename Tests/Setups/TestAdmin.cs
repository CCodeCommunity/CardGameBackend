using System;
using Api.Services;
using Tests.Helpers;
using System.Threading.Tasks;
using Api;
using Api.Enums;
using Api.Models;
using Api.Utilities;

namespace Tests.Setups;

public class TestAdmin : IDisposable
{
    public const string Name = "Admin";
    public const string Email = "admin@cardgame.cc";
    public const string Password = "maoisgay";

    private readonly IntegrationTestEnvironment env;

    public Account Value = null!;

    public static async Task<TestAdmin> MakeAsync(IntegrationTestEnvironment env)
    {
        var result = new TestAdmin(env);
        await result.Setup();
        return result;
    }
    
    private TestAdmin(IntegrationTestEnvironment env)
    {
        this.env = env;
    }

    public async Task Setup()
    {
        using var db = ScopedService<DatabaseContext>.GetService(env.App);

        Value = new Account
        {
            Id = await Nanoid.Nanoid.GenerateAsync(),
            Name = Name,
            Email = Email,
            Password = await Argon2Utils.HashPassword(Password),
            Role = AccountRole.Admin,
            State = AccountState.Active
        };
        
        await db.Service.Accounts.AddAsync(Value);
        await db.Service.SaveChangesAsync();
    }

    public string MakeAccessToken()
    {
        using var tokenService = ScopedService<AuthTokenService>.GetService(env.App);
        var token = tokenService.Service.GenerateAccessToken(Value);
        return token!;
    }
    
    public async Task RemoveAsync()
    {
        using var db = ScopedService<DatabaseContext>.GetService(env.App);

        await db.Service.Accounts.DeleteByKeyAsync(Value.Id);
    }

    public void Dispose()
    {
        using var db = ScopedService<DatabaseContext>.GetService(env.App);

        db.Service.Accounts.DeleteByKey(Value.Id);
    }
}