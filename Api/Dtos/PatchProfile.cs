using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public static class PatchProfile
{
    public record Request(string? Name, [Url] string? ProfilePicture);
}