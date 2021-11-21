using Api.Enums;
using Api.Models;

namespace Api.Dtos;

public static class PatchCard
{
    public record Request(
        string? Name = null,
        CardType? Type = null,
        int? CardCollectionId = null,
        CardRarity? Rarity = null,
        int? Attack = null,
        int? Health = null,
        int? Cost = null,
        string? Description = null,
        string? Quote = null,
        string? QuoteUrl = null,
        string? ImageUrl = null,
        string? HearthstoneEquivalentUrl = null
    );
}