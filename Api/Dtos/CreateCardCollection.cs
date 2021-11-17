namespace Api.Dtos;

public static class CreateCardCollection
{
    public record Request(string Name, string ImageUrl);
}