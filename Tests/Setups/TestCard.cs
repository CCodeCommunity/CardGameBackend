using System;
using System.Threading.Tasks;
using Api;
using Api.Enums;
using Api.Models;
using Tests.Helpers;

namespace Tests.Setups;

public class TestCard : IDisposable
{
    private readonly IntegrationTestEnvironment env;
    public Card Value = null!;

    public static async Task<TestCard> MakeAsync(IntegrationTestEnvironment env, string creatorId)
    {
        var result = new TestCard(env);
        await result.Setup(creatorId);
        return result;
    }
    
    private TestCard(IntegrationTestEnvironment env)
    {
        this.env = env;
    }

    public async Task Setup(string creatorId)
    {
        using var dbService = ScopedService<DatabaseContext>.GetService(env.App);
        var db = dbService.Service;
        
        Value = new Card
        {
            Name = "Test",
            Type = CardType.Censor,
            Rarity = CardRarity.Rare,
            Attack = 0,
            Health = 0,
            Cost = 0,
            Description = "Test",
            Quote = "Test",
            QuoteUrl = "test.com",
            ImageUrl = "test.com",
            HearthstoneEquivalentUrl = "test.com",
            CreatorId = creatorId
        };

        await db.Cards.AddAsync(Value);

        await db.SaveChangesAsync();
    }
    
    public async Task RemoveAsync()
    {
        using var db = ScopedService<DatabaseContext>.GetService(env.App);

        await db.Service.Cards.DeleteByKeyAsync(Value.Id);
    }
    
    public void Dispose()
    {
        using var db = ScopedService<DatabaseContext>.GetService(env.App);

        db.Service.Cards.DeleteByKey(Value.Id);
    }
}