using Api.Enums;

namespace Api.Dtos;

public static class CreateCard
{
    public record Request(
        string Name,
        CardType Type,
        int? CardCollectionId,
        CardRarity Rarity,
        int Attack,
        int Health,
        int Cost,
        string Description,
        string Quote,
        string QuoteUrl,
        string ImageUrl,
        string? HearthstoneEquivalentUrl,
        string CreatorId
    );
}