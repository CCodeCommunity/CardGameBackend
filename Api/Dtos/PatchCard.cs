using Api.Enums;
using Api.Models;

namespace Api.Dtos;

public static class PatchCard
{
    public record Request(
        string? Name,
        CardType? Type,
        DebaterType? InvokeableBy,
        int? CardCollectionId,
        CardRarity? Rarity,
        int? Attack,
        int? Health,
        int? Cost,
        string? Description,
        string? Quote,
        string? QuoteUrl,
        string? ImageUrl,
        string? HearthstoneEquivalentUrl,
        CardBehaviour? Behaviour
    );
}