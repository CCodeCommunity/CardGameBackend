namespace Api.Dtos;

public static class Logout
{
    public record Request(string RefreshToken, string AccessToken);
}