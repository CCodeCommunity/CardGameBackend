using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Models;

[Index(nameof(Token), IsUnique = true)]
[Index(nameof(AccountId), IsUnique = false)]
public class LoginInstance
{
    [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required] public int Id { get; set; }
    
    [Required] public string Token { get; set; } = null!;

    [Required] public string Device { get; set; } = null!;
        
    [Required] public string DeviceAgent { get; set; } = null!;
        
    [Required] public string DeviceOS { get; set; } = null!;
        
    [Required] public string Ip { get; set; } = null!;
        
    [Required] public DateTime CreatedAt { get; set; }
    
    [Required] public DateTime LastTokenGrantedAt { get; set; }
    
    [Required] public string AccountId { get; set; } = null!;
        
    [Required] public Account Account { get; set; } = null!;
    
    [Required] public LoginInstanceState State { get; set; }
}