using System.Collections.Generic;
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

public class CollectionsControllerTests
{
    [Fact]
    public async Task GetAllCardCollections_TestAuthorizationAndAuthentication()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        // Act and Assert
        await env.TestEndpointRequiresAuthenticationAsync(HttpVerb.Get, "https://localhost:5001/api/collections");
        await env.TestEndpointRoleAuthorizationAsync(HttpVerb.Get, "https://localhost:5001/api/collections", TestRoleAuthorizationScenario.UserAndAdmin);
    }
    
    [Fact]
    public async Task GetCollection_TestAuthorizationAndAuthentication()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        // Act and Assert
        await env.TestEndpointRequiresAuthenticationAsync(HttpVerb.Get, "https://localhost:5001/api/collections/0");
        await env.TestEndpointRoleAuthorizationAsync(HttpVerb.Get, "https://localhost:5001/api/collections/0", TestRoleAuthorizationScenario.UserAndAdmin);
    }
    
    [Fact]
    public async Task CreateCollection_TestAuthorizationAndAuthentication()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        // Act and Assert
        await env.TestEndpointRequiresAuthenticationAsync(HttpVerb.Post, "https://localhost:5001/api/collections");
        await env.TestEndpointRoleAuthorizationAsync(HttpVerb.Post, "https://localhost:5001/api/collections", TestRoleAuthorizationScenario.AdminOnly);
    }
    
    [Fact]
    public async Task PatchCollection_TestAuthorizationAndAuthentication()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        // Act and Assert
        await env.TestEndpointRequiresAuthenticationAsync(HttpVerb.Patch, "https://localhost:5001/api/collections/0");
        await env.TestEndpointRoleAuthorizationAsync(HttpVerb.Patch, "https://localhost:5001/api/collections/0", TestRoleAuthorizationScenario.AdminOnly);
    }
    
    [Fact]
    public async Task DeleteCollection_TestAuthorizationAndAuthentication()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        // Act and Assert
        await env.TestEndpointRequiresAuthenticationAsync(HttpVerb.Delete, "https://localhost:5001/api/collections/0");
        await env.TestEndpointRoleAuthorizationAsync(HttpVerb.Delete, "https://localhost:5001/api/collections/0", TestRoleAuthorizationScenario.AdminOnly);
    }
    
    [Fact]
    public async Task GetAllCardCollections_Authenticated_ExpectNone()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());
        
        // Act
        var response = await env.Client.GetAsync("https://localhost:5001/api/collections");
        var result = await response.ParseJsonAsync<List<CardCollection>>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetAllCardCollections_Authenticated_ExpectOne()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());

        var collection = await TestCollection.MakeAsync(env, user.Value.Id);

        // Act
        var response = await env.Client.GetAsync("https://localhost:5001/api/collections");
        var result = await response.ParseJsonAsync<List<CardCollection>>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        result.Should().BeOfType<List<CardCollection>>()
            .Which.Count.Should().Be(1);
        
        result.Should().BeOfType<List<CardCollection>>()
            .Which.Exists(it => it.CreatorId == user.Value.Id).Should().BeTrue();
        
        result.Should().BeOfType<List<CardCollection>>()
            .Which.Exists(it => it.Name == collection.Value.Name).Should().BeTrue();
        
        result.Should().BeOfType<List<CardCollection>>()
            .Which.Exists(it => it.ImageUrl == collection.Value.ImageUrl).Should().BeTrue();
    }
    
    [Fact]
    public async Task GetSpecificCardCollection_Authenticated_ExpectOne()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());

        var collection = await TestCollection.MakeAsync(env, user.Value.Id);
        
        // Act
        var response = await env.Client.GetAsync($"https://localhost:5001/api/collections/{collection.Value.Id}");
        var result = await response.ParseJsonAsync<CardCollection>();
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        result.Should().BeOfType<CardCollection>()
            .Which.CreatorId.Should().Be(user.Value.Id);
        
        result.Should().BeOfType<CardCollection>()
            .Which.Name.Should().Be(collection.Value.Name);

        result.Should().BeOfType<CardCollection>()
            .Which.ImageUrl.Should().Be(collection.Value.ImageUrl);
    }
    
    [Fact]
    public async Task GetSpecificCardCollection_WrongId_ExpectOne()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());

        var collection = await TestCollection.MakeAsync(env, user.Value.Id);
        
        // Act
        var response = await env.Client.GetAsync($"https://localhost:5001/api/collections/{collection.Value.Id + 1}"); // Wrong Id since we increment by one

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
    
    [Fact]
    public async Task CreateCollection_AsUser_ExpectForbidden()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());
        
        // Act
        var response = await env.Client.PostAsJsonAsync($"https://localhost:5001/api/collections", new {});

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task CreateCollection_AsAdmin_NoArgs_ExpectBadRequest()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var user = await TestAdmin.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());
        
        // Act
        var response = await env.Client.PostAsJsonAsync($"https://localhost:5001/api/collections", new {});

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task CreateCollection_AsAdmin_ExpectSuccess()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var user = await TestAdmin.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());

        var collection = new CreateCardCollection.Request(Name: "Test", ImageUrl: "Test");

        // Act
        var createCollectionResponse = await env.Client.PostAsJsonAsync($"https://localhost:5001/api/collections", collection);
        var createCollectionResult = await createCollectionResponse.ParseJsonAsync<CardCollection>(); 
        var getCollectionResponse = await env.Client.GetAsync($"https://localhost:5001/api/collections");
        var getCollectionResult = await getCollectionResponse.ParseJsonAsync<List<CardCollection>>();

        // Assert
        createCollectionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        createCollectionResult.Should().BeOfType<CardCollection>()
            .Which.CreatorId.Should().Be(user.Value.Id);
        
        createCollectionResult.Should().BeOfType<CardCollection>()
            .Which.Name.Should().Be(collection.Name);

        createCollectionResult.Should().BeOfType<CardCollection>()
            .Which.ImageUrl.Should().Be(collection.ImageUrl);
        
        getCollectionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        getCollectionResult.Should().BeOfType<List<CardCollection>>()
            .Which.Count.Should().Be(1);
        
        getCollectionResult.Should().BeOfType<List<CardCollection>>()
            .Which.Exists(it => it.CreatorId == user.Value.Id).Should().BeTrue();
        
        getCollectionResult.Should().BeOfType<List<CardCollection>>()
            .Which.Exists(it => it.Name == collection.Name).Should().BeTrue();
        
        getCollectionResult.Should().BeOfType<List<CardCollection>>()
            .Which.Exists(it => it.ImageUrl == collection.ImageUrl).Should().BeTrue();
    }
    
    [Fact]
    public async Task PatchCollectionName_AsAdmin_ExpectSuccess()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var user = await TestAdmin.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());

        var collection = await TestCollection.MakeAsync(env, user.Value.Id);

        // Act
        var patchCollectionResponse = await env.Client.PatchAsJsonAsync(
            $"https://localhost:5001/api/collections/{collection.Value.Id}", 
            new PatchCardCollection.Request(Name: "Test1", ImageUrl: null));
        
        var getCollectionResponse = await env.Client.GetAsync($"https://localhost:5001/api/collections");
        var getCollectionResult = await getCollectionResponse.ParseJsonAsync<List<CardCollection>>();

        // Assert
        patchCollectionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        getCollectionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        getCollectionResult.Should().BeOfType<List<CardCollection>>()
            .Which.Count.Should().Be(1);
        
        getCollectionResult.Should().BeOfType<List<CardCollection>>()
            .Which.Exists(it => it.CreatorId == user.Value.Id).Should().BeTrue();
        
        getCollectionResult.Should().BeOfType<List<CardCollection>>()
            .Which.Exists(it => it.Name == "Test1").Should().BeTrue();
        
        getCollectionResult.Should().BeOfType<List<CardCollection>>()
            .Which.Exists(it => it.ImageUrl == collection.Value.ImageUrl).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCollection_AsAdmin_ExpectForbidden()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var admin = await TestAdmin.MakeAsync(env);
        env.SetAccessToken(admin.MakeAccessToken());

        var collection = await TestCollection.MakeAsync(env, admin.Value.Id);
        
        // Act
        var deleteCollectionResponse = await env.Client.DeleteAsync($"https://localhost:5001/api/collections/{collection.Value.Id}");
        
        // Assert
        deleteCollectionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}