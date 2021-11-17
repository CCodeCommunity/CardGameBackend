using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Api.Dtos;
using Api.Models;
using Api.Services;
using Api.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Tests.Helpers;
using Tests.Setups;
using Xunit;

namespace Tests;

public class AccountsControllerTests
{
    [Fact]
    public async Task CreateAccount_WithoutArguments_ReturnsBadResponse()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var request = new { };
        
        // Act
        var result = await env.Client.PostAsJsonAsync("https://localhost:5001/api/accounts", request);
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task CreateAccount_WithInvalidEmail_ReturnsBadResponse()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var request = new CreateAccount.Request(Name: "Test", Email: "test", Password: "1234123");
        
        // Act
        var result = await env.Client.PostAsJsonAsync("https://localhost:5001/api/accounts", request);
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task CreateAccount_WithTooShortOfAPassword_ReturnsBadResponse()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var request = new CreateAccount.Request(Name: "Test", Email: "test@test.io", Password: "1234");
        
        // Act
        var result = await env.Client.PostAsJsonAsync("https://localhost:5001/api/accounts", request);
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create2Accounts_WithSameEmail_FirstSuccessfulSecondUnsuccessful()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var request1 = new CreateAccount.Request(Name: "Test", Email: "test@test.io", Password: "1234123");
        var request2 = new CreateAccount.Request(Name: "Test", Email: "test@test.io", Password: "1234123");
        
        // Act
        var result1 = await env.Client.PostAsJsonAsync("https://localhost:5001/api/accounts", request1);
        var result2 = await env.Client.PostAsJsonAsync("https://localhost:5001/api/accounts", request2);
        
        // Assert
        result1.StatusCode.Should().Be(HttpStatusCode.OK);
        result2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create2Accounts_WithDifferentEmails_BothSuccessful()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var request1 = new CreateAccount.Request(Name: "Test", Email: "test1@test.io", Password: "1234123");
        var request2 = new CreateAccount.Request(Name: "Test", Email: "test2@test.io", Password: "1234123");
        
        // Act
        var result1 = await env.Client.PostAsJsonAsync("https://localhost:5001/api/accounts", request1);
        var result2 = await env.Client.PostAsJsonAsync("https://localhost:5001/api/accounts", request2);
        
        // Assert
        result1.StatusCode.Should().Be(HttpStatusCode.OK);
        result2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LoginUser_GetNewAccessToken_ReturnSuccessful()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var user = new TestUser(env);
        await user.Setup();
        
        var request = new Login.Request(
            Email: user.Account.Email, 
            Password: TestUser.Password, 
            Device: "Test",
            DeviceAgent: "Test", 
            DeviceOS: "Test");

        // Act
        var loginResponse = await env.Client.PostAsJsonAsync("https://localhost:5001/api/accounts/login", request);
        var loginResult = (await loginResponse.Content.ReadFromJsonAsync<Login.Response>())!;
        
        var getAccessTokenResponse = await env.Client.PatchAsJsonAsync("https://localhost:5001/api/accounts/access-token",
            new GetAccessToken.Request(RefreshToken: loginResult.RefreshToken, AccessToken: loginResult.AccessToken));
        var getAccessTokenResult = (await getAccessTokenResponse.Content.ReadFromJsonAsync<GetAccessToken.Response>())!;
        
        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        
        getAccessTokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getAccessTokenResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
        getAccessTokenResult.AccessToken.Should().NotBeNullOrWhiteSpace();
    }
    
    [Fact]
    public async Task LoginUser_GetNewAccessToken_TryOldAccessToken_BothTokensAreNowInvalid()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        var user = new TestUser(env);
        await user.Setup();
        
        var request = new Login.Request(
            Email: user.Account.Email, 
            Password: TestUser.Password, 
            Device: "Test",
            DeviceAgent: "Test", 
            DeviceOS: "Test");

        // Act
        var loginResponse = await env.Client.PostAsJsonAsync("https://localhost:5001/api/accounts/login", request);
        var loginResult = (await loginResponse.Content.ReadFromJsonAsync<Login.Response>())!;
        
        var getAccessTokenResponse = await env.Client.PatchAsJsonAsync("https://localhost:5001/api/accounts/access-token",
            new GetAccessToken.Request(RefreshToken: loginResult.RefreshToken, AccessToken: loginResult.AccessToken));
        var getAccessTokenResult = (await getAccessTokenResponse.Content.ReadFromJsonAsync<GetAccessToken.Response>())!;
        
        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        
        getAccessTokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getAccessTokenResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
        getAccessTokenResult.AccessToken.Should().NotBeNullOrWhiteSpace();
    }
    
    [Fact]
    public async Task CreateAccount_GetListForAdmin_ReturnsSuccessful()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        var admin = new TestAdmin(env);
        await admin.Setup();
        
        var loginRequest = new Login.Request(Email: admin.Account.Email, Password: TestUser.Password, Device: "Test", DeviceAgent: "Test", DeviceOS: "Test");
        var createAccountRequest = new CreateAccount.Request(Name: "Test", Email: "test1@test.io", Password: "1234123");

        // Act
        var createAccountResponse = await env.Client.PostAsJsonAsync("https://localhost:5001/api/accounts", createAccountRequest);
        var loginResponse = await env.Client.PostAsJsonAsync("https://localhost:5001/api/accounts/login", loginRequest);
        var loginResult = (await loginResponse.ParseJsonAsync<Login.Response>())!;
        env.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
        var getAccountsResponse = await env.Client.GetAsync($"https://localhost:5001/api/accounts?page=1&pageSize=5");
        var getAccountsResult = (await getAccountsResponse.ParseJsonAsync<PagedResult<Account>>())!;
        
        // Assert
        createAccountResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getAccountsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getAccountsResult.RowCount.Should().Be(2);
        getAccountsResult.Results.First().Should().BeOfType<Account>().Which.Name.Should().Be(admin.Account.Name);
        getAccountsResult.Results.Last().Should().BeOfType<Account>().Which.Name.Should().Be(createAccountRequest.Name);
    }
    
    [Fact]
    public async Task GetAccountList_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        
        // Act
        var getAccountsResponse = await env.Client.GetAsync($"https://localhost:5001/api/accounts?page=1&pageSize=5");

        // Assert
        getAccountsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task GetAccountList_WithRegularUser_ReturnsForbidden()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();

        var user = new TestUser(env);
        await user.Setup();
        
        var loginRequest = new Login.Request(Email: user.Account.Email, Password: TestUser.Password, Device: "Test", DeviceAgent: "Test", DeviceOS: "Test");

        // Act
        var loginResponse = await env.Client.PostAsJsonAsync("https://localhost:5001/api/accounts/login", loginRequest);
        var loginResult = (await loginResponse.ParseJsonAsync<Login.Response>())!;
        env.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
        
        var getAccountsResponse = await env.Client.GetAsync($"https://localhost:5001/api/accounts?page=1&pageSize=5");

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getAccountsResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task GetAccountList_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        using var env = new IntegrationTestEnvironment();
        using var config = ScopedService<IConfiguration>.GetService(env.App);
        
        var admin = new TestAdmin(env);
        await admin.Setup();
        
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(s => s["Jwt:Key"]).Returns(config.Service["Jwt:Key"]);
        mockConfig.Setup(s => s["Jwt:Issuer"]).Returns(config.Service["Jwt:Issuer"]);
        mockConfig.Setup(s => s["Jwt:AccessTokenExpiryInMinutes"]).Returns("0");
        var tokenService = new AuthTokenService(mockConfig.Object);
        
        var accessToken = tokenService.GenerateAccessToken(admin.Account);
        env.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var getAccountsResponse = await env.Client.GetAsync($"https://localhost:5001/api/accounts?page=1&pageSize=5");

        // Assert
        getAccountsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}