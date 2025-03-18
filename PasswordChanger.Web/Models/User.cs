using System.ComponentModel.DataAnnotations;

namespace PasswordChanger.Web.Models;

public class User
{
    [Required]
    public string Username { get; set; } = null!;
    
    public string? Email { get; set; }
    
    public string? Otp { get; set; }
    
    public DateTime OtpTimeStamp { get; set; }
}