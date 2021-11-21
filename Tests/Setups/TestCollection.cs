using System.Threading.Tasks;
using Api;
using Api.Models;
using Tests.Helpers;

namespace Tests.Setups;

public class TestCollection
{
    public const string Name = "Test";
    public const string ImageUrl = "Test";

    private readonly IntegrationTestEnvironment env;
    public CardCollection Value = null!;

    private TestCollection(IntegrationTestEnvironment env)
    {
        this.env = env;
    }
    
    public static async Task<TestCollection> MakeAsync(IntegrationTestEnvironment env, string creatorId)
    {
        var result = new TestCollection(env);
        await result.Setup(creatorId);
        return result;
    }

    public async Task Setup(string creatorId)
    {
        using var dbService = ScopedService<DatabaseContext>.GetService(env.App);
        var db = dbService.Service;
        
        Value = new CardCollection
        {
            Name = Name,
            ImageUrl = "test",
            CreatorId = creatorId,
        };

        await db.CardCollections.AddAsync(Value);

        await db.SaveChangesAsync();
    }
}