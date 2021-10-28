// Disable null checks here as they are not necessary because we know these will be initialized by EF
#pragma warning disable 8618

using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Card
    {
        [Required] public int Id;
        [Required] public string Name;
        [Required] public CardType Type;
        [Required] public DebaterType InvokeableBy;
        [Required] public Collection Collection; 
        [Required] public CardRarity Rarity;
        [Required] public int Attack;
        [Required] public int Health;
        [Required] public int Cost;
        [Required] public string Description;
        [Required] public string Quote;
        [Required] public string QuoteURL;
        [Required] public string ImageURL;
        [Required] public CardBehaviour Behaviour;
        [Required] public Account Creator;
    }
}