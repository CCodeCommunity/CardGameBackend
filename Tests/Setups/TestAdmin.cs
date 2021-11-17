using Tests.Helpers;

namespace Tests.Setups;

using System.Threading.Tasks;
using Api;
using Api.Enums;
using Api.Models;
using Api.Utilities;

public class TestAdmin
{
    public const string Name = "Admin";
    public const string Email = "admin@cardgame.cc";
    public const string Password = "maoisgay";

    private readonly IntegrationTestEnvironment env;

    public Account Account = null!;

    public TestAdmin(IntegrationTestEnvironment env)
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
            Role = AccountRole.Admin,
            State = AccountState.Active
        };

        await db.Accounts.AddAsync(Account);

        await db.SaveChangesAsync();
    }
}