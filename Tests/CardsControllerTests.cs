using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Api.Dtos;
using Api.Enums;
using Api.Models;
using Api.Utilities;
using FluentAssertions;
using Tests.Helpers;
using Tests.Setups;
using Xunit;

namespace Tests;

public class CardsControllerTests
{
    [Fact]
    public async Task CountCards_TestAuthorizationAndAuthentication()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        // Act and Assert
        await env.TestEndpointRequiresAuthenticationAsync(HttpVerb.Get, 
            "https://localhost:5001/api/cards/count");
        await env.TestEndpointRoleAuthorizationAsync(HttpVerb.Get, 
            "https://localhost:5001/api/cards/count", TestRoleAuthorizationScenario.UserAndAdmin);
    }
    
    [Fact]
    public async Task GetCard_TestAuthorizationAndAuthentication()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        // Act and Assert
        await env.TestEndpointRequiresAuthenticationAsync(HttpVerb.Get, 
            "https://localhost:5001/api/cards/0");
        await env.TestEndpointRoleAuthorizationAsync(HttpVerb.Get, 
            "https://localhost:5001/api/cards/0", TestRoleAuthorizationScenario.UserAndAdmin);
    }
    
    [Fact]
    public async Task GetCards_TestAuthorizationAndAuthentication()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        // Act and Assert
        await env.TestEndpointRequiresAuthenticationAsync(HttpVerb.Get, 
            "https://localhost:5001/api/cards?page=1&pageSize=1");
        await env.TestEndpointRoleAuthorizationAsync(HttpVerb.Get, 
            "https://localhost:5001/api/cards?page=1&pageSize=1", TestRoleAuthorizationScenario.UserAndAdmin);
    }
    
    [Fact]
    public async Task DeleteCard_TestAuthorizationAndAuthentication()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        // Act and Assert
        await env.TestEndpointRequiresAuthenticationAsync(HttpVerb.Delete, 
            "https://localhost:5001/api/cards/0");
        await env.TestEndpointRoleAuthorizationAsync(HttpVerb.Delete, 
            "https://localhost:5001/api/cards/0", TestRoleAuthorizationScenario.AdminOnly);
    }
    
    [Fact]
    public async Task CreateCard_TestAuthorizationAndAuthentication()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        // Act and Assert
        await env.TestEndpointRequiresAuthenticationAsync(HttpVerb.Post, 
            "https://localhost:5001/api/cards");
        await env.TestEndpointRoleAuthorizationAsync(HttpVerb.Post, 
            "https://localhost:5001/api/cards", TestRoleAuthorizationScenario.UserAndAdmin);
    }
    
    [Fact]
    public async Task PatchCard_TestAuthorizationAndAuthentication()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        // Act and Assert
        await env.TestEndpointRequiresAuthenticationAsync(HttpVerb.Patch, 
            "https://localhost:5001/api/cards/0");
        await env.TestEndpointRoleAuthorizationAsync(HttpVerb.Patch, 
            "https://localhost:5001/api/cards/0", TestRoleAuthorizationScenario.UserAndAdmin);
    }
    
    [Fact]
    public async Task CountCards_Expect0()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        using var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());

        // Act
        var countResponse = await env.Client.GetAsync("https://localhost:5001/api/cards/count");
        var countResult = await countResponse.ParseJsonAsync<CardsCount.Response>();

        // Assert
        countResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        countResult.Should().BeOfType<CardsCount.Response>().Which.Count.Should().Be(0);
    }
    
    [Fact]
    public async Task GetCard_ExpectUnprocessableEntity()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        using var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());

        // Act
        var getCardResponse = await env.Client.GetAsync("https://localhost:5001/api/cards/1");

        // Assert
        getCardResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
    
    [Fact]
    public async Task GetCard_ExpectSuccess()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        using var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());

        using var card = await TestCard.MakeAsync(env, user.Value.Id);

        // Act
        var getCardResponse = await env.Client.GetAsync($"https://localhost:5001/api/cards/{card.Value.Id}");
        var getCardResult = await getCardResponse.ParseJsonAsync<Card>();

        // Assert
        getCardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getCardResult.Should().BeOfType<Card>().Which.Id.Should().Be(card.Value.Id);
        getCardResult.Should().BeOfType<Card>().Which.Type.Should().Be(card.Value.Type);
        getCardResult.Should().BeOfType<Card>().Which.Rarity.Should().Be(card.Value.Rarity);
        getCardResult.Should().BeOfType<Card>().Which.Attack.Should().Be(card.Value.Attack);
        getCardResult.Should().BeOfType<Card>().Which.Health.Should().Be(card.Value.Health);
        getCardResult.Should().BeOfType<Card>().Which.Cost.Should().Be(card.Value.Cost);
        getCardResult.Should().BeOfType<Card>().Which.Description.Should().Be(card.Value.Description);
        getCardResult.Should().BeOfType<Card>().Which.Quote.Should().Be(card.Value.Quote);
        getCardResult.Should().BeOfType<Card>().Which.QuoteUrl.Should().Be(card.Value.QuoteUrl);
        getCardResult.Should().BeOfType<Card>().Which.ImageUrl.Should().Be(card.Value.ImageUrl);
        getCardResult.Should().BeOfType<Card>().Which.HearthstoneEquivalentUrl.Should().Be(card.Value.HearthstoneEquivalentUrl);
        getCardResult.Should().BeOfType<Card>().Which.CreatorId.Should().Be(card.Value.CreatorId);
    }
    
    [Fact]
    public async Task GetCards_Page0_ExpectBadRequest()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        using var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());

        // Act
        var getCardsResponse = await env.Client.GetAsync("https://localhost:5001/api/cards?page=0&pageSize=0");

        // Assert
        getCardsResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetCards_Page1_ExpectSuccess()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        using var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());
        
        using var card = await TestCard.MakeAsync(env, user.Value.Id);

        // Act
        var getCardsResponse = await env.Client.GetAsync("https://localhost:5001/api/cards?page=1&pageSize=1");
        var getCardResult = await getCardsResponse.ParseJsonAsync<PagedResult<Card>>();

        // Assert
        getCardsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getCardResult.Should().BeOfType<PagedResult<Card>>().Which.CurrentPage.Should().Be(1);
    }
    
    [Fact]
    public async Task DeleteCard_ExpectSuccess()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        using var admin = await TestAdmin.MakeAsync(env);
        env.SetAccessToken(admin.MakeAccessToken());
        
        using var card = await TestCard.MakeAsync(env, admin.Value.Id);

        // Act
        var count1Response = await env.Client.GetAsync("https://localhost:5001/api/cards/count");
        var count1Result = await count1Response.ParseJsonAsync<CardsCount.Response>();
        var deleteCard = await env.Client.DeleteAsync($"https://localhost:5001/api/cards/{card.Value.Id}");
        var count2Response = await env.Client.GetAsync("https://localhost:5001/api/cards/count");
        var count2Result = await count2Response.ParseJsonAsync<CardsCount.Response>();

        // Assert
        deleteCard.StatusCode.Should().Be(HttpStatusCode.OK);
        count1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        count2Response.StatusCode.Should().Be(HttpStatusCode.OK);
        count1Result.Should().BeOfType<CardsCount.Response>().Which.Count.Should().Be(1);
        count2Result.Should().BeOfType<CardsCount.Response>().Which.Count.Should().Be(0);
    }
    
    [Fact]
    public async Task DeleteNonexistentCard_ExpectSuccess()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        using var admin = await TestAdmin.MakeAsync(env);
        env.SetAccessToken(admin.MakeAccessToken());
        
        // Act
        var deleteCard = await env.Client.DeleteAsync($"https://localhost:5001/api/cards/0");

        // Assert
        deleteCard.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
    
    [Fact]
    public async Task CreateCard_ExpectSuccess()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        using var admin = await TestAdmin.MakeAsync(env);
        env.SetAccessToken(admin.MakeAccessToken());
        
        var card = new CreateCard.Request(
            Name: "Test",
            Type: CardType.Censor,
            CardCollectionId: null,
            Rarity: CardRarity.Rare,
            Attack: 0,
            Health: 0,
            Cost: 0,
            Description: "Test",
            Quote: "Test",
            QuoteUrl: "test.com",
            ImageUrl: "test.com",
            HearthstoneEquivalentUrl: "test.com",
            CreatorId: admin.Value.Id
        );

        // Act
        var count1Response = await env.Client.GetAsync("https://localhost:5001/api/cards/count");
        var count1Result = await count1Response.ParseJsonAsync<CardsCount.Response>();
        var createCardResponse = await env.Client.PostAsJsonAsync($"https://localhost:5001/api/cards", card);
        var createCardResult = await createCardResponse.ParseJsonAsync<Card>();
        var count2Response = await env.Client.GetAsync("https://localhost:5001/api/cards/count");
        var count2Result = await count2Response.ParseJsonAsync<CardsCount.Response>();

        // Assert
        createCardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        count1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        count2Response.StatusCode.Should().Be(HttpStatusCode.OK);
        count1Result.Should().BeOfType<CardsCount.Response>().Which.Count.Should().Be(0);
        count2Result.Should().BeOfType<CardsCount.Response>().Which.Count.Should().Be(1);
        
        createCardResult.Should().BeOfType<Card>().Which.Id.Should().Be(1);
        createCardResult.Should().BeOfType<Card>().Which.Name.Should().Be(card.Name);
        createCardResult.Should().BeOfType<Card>().Which.Type.Should().Be(card.Type);
        createCardResult.Should().BeOfType<Card>().Which.Rarity.Should().Be(card.Rarity);
        createCardResult.Should().BeOfType<Card>().Which.Attack.Should().Be(card.Attack);
        createCardResult.Should().BeOfType<Card>().Which.Health.Should().Be(card.Health);
        createCardResult.Should().BeOfType<Card>().Which.Cost.Should().Be(card.Cost);
        createCardResult.Should().BeOfType<Card>().Which.Description.Should().Be(card.Description);
        createCardResult.Should().BeOfType<Card>().Which.Quote.Should().Be(card.Quote);
        createCardResult.Should().BeOfType<Card>().Which.QuoteUrl.Should().Be(card.QuoteUrl);
        createCardResult.Should().BeOfType<Card>().Which.ImageUrl.Should().Be(card.ImageUrl);
        createCardResult.Should().BeOfType<Card>().Which.HearthstoneEquivalentUrl.Should().Be(card.HearthstoneEquivalentUrl);
        createCardResult.Should().BeOfType<Card>().Which.CreatorId.Should().Be(card.CreatorId);
    }
    
    [Fact]
    public async Task PatchCard_ExpectSuccess()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        using var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());

        var card = await TestCard.MakeAsync(env, user.Value.Id);

        // Act
        var patchCardResponse = await env.Client.PatchAsJsonAsync($"https://localhost:5001/api/cards/{card.Value.Id}", new PatchCard.Request(Name: "NewName"));
        var getCardResponse = await env.Client.GetAsync($"https://localhost:5001/api/cards/{card.Value.Id}");
        var getCardResult = await getCardResponse.ParseJsonAsync<Card>();
        
        // Assert
        patchCardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        getCardResult.Should().BeOfType<Card>().Which.Id.Should().Be(card.Value.Id);
        getCardResult.Should().BeOfType<Card>().Which.Name.Should().Be("NewName");
    }
    
    [Fact]
    public async Task PatchCard_WrongUser_ExpectForbidden()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        using var admin = await TestAdmin.MakeAsync(env);
        using var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());

        var card = await TestCard.MakeAsync(env, admin.Value.Id);

        // Act
        var patchCardResponse = await env.Client.PatchAsJsonAsync($"https://localhost:5001/api/cards/{card.Value.Id}", new {});
        
        // Assert
        patchCardResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task PatchCard_WrongUserButAdmin_ExpectSuccess()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        using var admin = await TestAdmin.MakeAsync(env);
        using var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(admin.MakeAccessToken());

        var card = await TestCard.MakeAsync(env, user.Value.Id);

        // Act
        var patchCardResponse = await env.Client.PatchAsJsonAsync($"https://localhost:5001/api/cards/{card.Value.Id}", new PatchCard.Request(Name: "NewName"));
        var getCardResponse = await env.Client.GetAsync($"https://localhost:5001/api/cards/{card.Value.Id}");
        var getCardResult = await getCardResponse.ParseJsonAsync<Card>();
        
        // Assert
        patchCardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getCardResult.Should().BeOfType<Card>().Which.Id.Should().Be(card.Value.Id);
        getCardResult.Should().BeOfType<Card>().Which.Name.Should().Be("NewName");
    }
}