using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class LoginInstance
{
    [Key] public string Token { get; set; } = null!;

    [Required] public string Device { get; set; } = null!;
        
    [Required] public string DeviceAgent { get; set; } = null!;
        
    [Required] public string DeviceOS { get; set; } = null!;
        
    [Required] public string Ip { get; set; } = null!;
        
    [Required] public DateTime CreatedAt { get; set; }
        
    [Required] public DateTime ExpiresAt { get; set; }
        
    [Required] public string AccountId { get; set; } = null!;
        
    [Required] public Account Account { get; set; } = null!;
}