using PasswordChanger.Web.Models;

namespace PasswordChanger.Web.Abstractions.Services;

public interface IUserService
{
    Task<string> FindUserAndGetSessionTokenAsync(string username);
    
    Task ChangeUserPasswordAsync(string token, string newPassword);
    
    Task<bool> ValidateOtpAsync(string username, string otp);

    Task<User> GetUserAndCheckSessionValidationAsync(string? userJson);

    Task<Contact> GetUserEmailAsync(string username);
}