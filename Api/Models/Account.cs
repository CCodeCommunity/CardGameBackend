using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Models;

[Index(nameof(Email), IsUnique = true)]
public class Account
{
    [Key, Required] public string Id { get; set; } = null!;

    [Required, StringLength(maximumLength: 1000, MinimumLength = 1)]
    public string Name { get; set; } = null!;

    [Url] public string? ProfilePicture { get; set; }

    [Required, EmailAddress] 
    public string Email { get; set; } = null!;

    [Required, MinLength(1), MaxLength(1000)]
    public string Password { get; set; } = null!;

    [Required] public AccountRole Role { get; set; }

    [Required] public AccountState State { get; set; }

    [Required] public List<Card> Cards { get; set; } = null!;

    [Required] public List<CardCollection> CardCollections { get; set; } = null!;
}