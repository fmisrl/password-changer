namespace PasswordChanger.Web.Models;

public class OtpSendRequest
{
    public string Otp { get; set; } = null!;

    public UserOtpSendRequest User { get; set; } = null!;
    
    public class UserOtpSendRequest
    {
        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }

        public string Email { get; set; } = null!;
    }
}