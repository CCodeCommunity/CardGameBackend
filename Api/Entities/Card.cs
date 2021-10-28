
using System.ComponentModel.DataAnnotations;

namespace Api.Entities
{
    public class Card
    {
        [Required] public int Id;
        [Required] public string Name;
        [Required] public CardType Type;
        [Required] public DebateType InvokeableBy;
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