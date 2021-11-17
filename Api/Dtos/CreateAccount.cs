using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public static class CreateAccount
{
    public record Request
    (
        [Required]
        [StringLength(maximumLength: 1000, MinimumLength = 1)]
        string Name,
            
        [Required]
        [EmailAddress]
        string Email,
            
        [Required]
        [StringLength(maximumLength: 300, MinimumLength = 6)]
        string Password
    );
}