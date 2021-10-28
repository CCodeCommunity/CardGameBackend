using System.ComponentModel.DataAnnotations;

namespace Api.Entities
{
    public class Collection
    {
        [Required] public int Id;
        [Required] public int Name;
        [Required] public int ImageUrl;
    }
}