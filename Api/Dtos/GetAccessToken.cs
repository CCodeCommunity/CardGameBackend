namespace Api.Dtos;

public static class GetAccessToken
{
    public record Request(string RefreshToken);
    public record Response(string RefreshToken, string AccessToken);
}