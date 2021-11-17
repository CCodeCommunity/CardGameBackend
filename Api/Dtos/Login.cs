namespace Api.Dtos;

public static class Login
{
    public record Request(
        string Email, 
        string Password,
        string Device,
        string DeviceAgent,
        string DeviceOS);

    public record Response(
        string RefreshToken, 
        string AccessToken);
}