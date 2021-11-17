namespace Api.Dtos;

public static class PatchCardCollection
{
    public record Request(string? Name, string? ImageUrl);
}