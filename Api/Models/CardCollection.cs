using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models;

public class CardCollection
{
    [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
    [Required] public int Id { get; set; }
    [Required] public string Name { get; set; } = null!;
    [Required] public string ImageUrl { get; set; } = null!;
    [Required] public string CreatorId { get; set; } = null!;
    [Required] [ForeignKey("CreatorId")] public Account Creator { get; set; } = null!;
}