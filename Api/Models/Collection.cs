// Disable null checks here as they are not necessary because we know these will be initialized by EF
#pragma warning disable 8618

using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Collection
    {
        [Required] public int Id;
        [Required] public string Name;
        [Required] public string ImageUrl;
    }
}