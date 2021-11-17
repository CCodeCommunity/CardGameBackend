using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Enums;

namespace Api.Models;

public class Card
{
    [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    [Required] public int Id { get; set; } 
    [Required] public string Name { get; set; } = null!; 
    [Required] public CardType Type { get; set; } 
    [Required] public DebaterType InvokeableBy { get; set; }
    [Required] public int CardCollectionId { get; set; }
    [Required] public CardCollection CardCollection { get; set; } = null!;  
    [Required] public CardRarity Rarity { get; set; } 
    [Required] public int Attack { get; set; } 
    [Required] public int Health { get; set; } 
    [Required] public int Cost { get; set; }
    [Required] public string Description { get; set; } = null!; 
    [Required] public string Quote { get; set; } = null!; 
    [Required] [Url] public string QuoteUrl { get; set; } = null!; 
    [Required] [Url] public string ImageUrl { get; set; } = null!;
    [Required] [Url] public string HearthstoneEquivalentUrl { get; set; } = null!;
    [Required] public CardBehaviour Behaviour { get; set; }
    [Required] public string CreatorId { get; set; } = null!;
    [Required] [ForeignKey("CreatorId")] public Account Creator { get; set; } = null!; 
}