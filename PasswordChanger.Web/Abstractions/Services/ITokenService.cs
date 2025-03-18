namespace PasswordChanger.Web.Abstractions.Services;

public interface ITokenService
{
    bool TryValidateOtp(string username, string otp);

    string GetOtp(string username);
    
    void DeleteOtp(string username);
}