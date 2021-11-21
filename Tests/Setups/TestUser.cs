using System;
using Api.Services;
using System.Threading.Tasks;
using Api;
using Api.Enums;
using Api.Models;
using Api.Utilities;
using Tests.Helpers;

namespace Tests.Setups;

public class TestUser : IDisposable
{
    public const string Name = "Test";
    public const string Email = "test@cardgame.cc";
    public const string Password = "maoisgay";

    private readonly IntegrationTestEnvironment env;
    public Account Value = null!;

    public static async Task<TestUser> MakeAsync(IntegrationTestEnvironment env)
    {
        var result = new TestUser(env);
        await result.Setup();
        return result;
    }
    
    private TestUser(IntegrationTestEnvironment env)
    {
        this.env = env;
    }

    public async Task Setup()
    {
        using var dbService = ScopedService<DatabaseContext>.GetService(env.App);
        var db = dbService.Service;
        
        Value = new Account
        {
            Id = await Nanoid.Nanoid.GenerateAsync(),
            Name = Name,
            Email = Email,
            Password = await Argon2Utils.HashPassword(Password),
            Role = AccountRole.User,
            State = AccountState.Active
        };

        await db.Accounts.AddAsync(Value);

        await db.SaveChangesAsync();
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