using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Api.Enums;
using Api.Utilities;
using FluentAssertions;
using Tests.Setups;

namespace Tests.Helpers;

public static class AuthTestingHelpers
{
    public static async Task<HttpResponseMessage> CallEndpointAsync(this IntegrationTestEnvironment env, HttpVerb verb, string endpoint)
    {
        return verb switch
        {
            HttpVerb.Delete => await env.Client.DeleteAsync(endpoint),
            HttpVerb.Get => await env.Client.GetAsync(endpoint),
            HttpVerb.Patch => await env.Client.PatchAsJsonAsync(endpoint, new { }),
            HttpVerb.Post => await env.Client.PostAsJsonAsync(endpoint, new { }),
            HttpVerb.Put => await env.Client.PutAsJsonAsync(endpoint, new { }),
            HttpVerb.Head => throw new ArgumentOutOfRangeException(nameof(verb), verb, null),
            HttpVerb.Options => throw new ArgumentOutOfRangeException(nameof(verb), verb, null),
            _ => throw new ArgumentOutOfRangeException(nameof(verb), verb, null)
        };
    }
    
    public static async Task TestEndpointRequiresAuthenticationAsync(this IntegrationTestEnvironment env, HttpVerb verb, string endpoint)
    {
        var result = await env.CallEndpointAsync(verb, endpoint);
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var user = await TestUser.MakeAsync(env);
        env.SetAccessToken(user.MakeAccessToken());
        result = await env.CallEndpointAsync(verb, endpoint);
        result.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
    
    public static async Task TestEndpointRoleAuthorizationAsync(this IntegrationTestEnvironment env, HttpVerb verb, string endpoint, TestRoleAuthorizationScenario scenario)
    {
        using var user = await TestUser.MakeAsync(env);
        using var admin = await TestAdmin.MakeAsync(env);
        
        env.SetAccessToken(user.MakeAccessToken());
        var withUserResponse = await env.CallEndpointAsync(verb, endpoint);
        
        env.SetAccessToken(admin.MakeAccessToken());
        var withAdminResponse = await env.CallEndpointAsync(verb, endpoint);

        if (scenario == TestRoleAuthorizationScenario.UserOnly)
        {
            withUserResponse.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
            withAdminResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        else if (scenario == TestRoleAuthorizationScenario.AdminOnly)
        {
            withUserResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            withAdminResponse.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }
        else if (scenario == TestRoleAuthorizationScenario.UserAndAdmin)
        {
            withUserResponse.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
            withAdminResponse.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }
    }
}