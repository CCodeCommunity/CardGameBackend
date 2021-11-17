namespace Tests.Setups;

using System.Threading.Tasks;
using Api;
using Api.Enums;
using Api.Models;
using Api.Utilities;
using Tests.Helpers;

public class TestUser
{
    public const string Name = "Test";
    public const string Email = "test@cardgame.cc";
    public const string Password = "maoisgay";

    private readonly IntegrationTestEnvironment env;
    public Account Account = null!;

    public TestUser(IntegrationTestEnvironment env)
    {
        this.env = env;
    }

    public async Task Setup()
    {
        using var dbService = ScopedService<DatabaseContext>.GetService(env.App);
        var db = dbService.Service;
        
        Account = new Account
        {
            Id = await Nanoid.Nanoid.GenerateAsync(),
            Name = Name,
            Email = Email,
            Password = await Argon2Utils.HashPassword(Password),
            Role = AccountRole.User,
            State = AccountState.Active
        };

        await db.Accounts.AddAsync(Account);

        await db.SaveChangesAsync();
    }
}