using System.Text.Json;
using PasswordChanger.Web.Abstractions.Services;
using PasswordChanger.Web.Exceptions;
using PasswordChanger.Web.Models;

namespace PasswordChanger.Web.Services;

public class UserService : IUserService
{
    private readonly ITokenService _tokenService;
    
    private readonly ILdapService _ldapService;
    
    private readonly IEmailService _emailService;
    
    public UserService(ITokenService tokenService, ILdapService ldapService, IEmailService emailService)
    {
        _tokenService = tokenService;
        _ldapService = ldapService;
        _emailService = emailService;
    }
    
    public async Task<string> FindUserAndGetSessionTokenAsync(string username)
    {
        var findTask = Task.Run(async () =>
        {
            var contact = _ldapService.GetUserContact(username);
            var otp = _tokenService.GetOtp(username);

            var otpSendRequest = new OtpSendRequest
            {
                Otp = otp,
                User = new OtpSendRequest.UserOtpSendRequest
                {
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    Email = contact.Email!
                }
            };
            
            await _emailService.SendOtpAsync(otpSendRequest);

            return otp;
        });

        return await findTask;
    }

    public async Task ChangeUserPasswordAsync(string username, string newPassword)
    {
        var changePasswordTask = Task.Run(() =>
        {
            _ldapService.ChangePassword(username, newPassword);
        });
        await changePasswordTask;
    }

    public async Task<bool> ValidateOtpAsync(string username, string otp)
    {
        var validationTask = Task.Run(() =>
        {
            var result = _tokenService.TryValidateOtp(username, otp);

            return result;
        });
        
        return await validationTask;
    }

    public async Task<User> GetUserAndCheckSessionValidationAsync(string? userJson)
    {
        var getUserTask = Task.Run(() =>
        {
            if (userJson is null) throw new SessionExpiredException();

            var user = JsonSerializer.Deserialize<User>(userJson);
            if (user == null) throw new SessionExpiredException();

            var timePassed = DateTime.UtcNow - user.OtpTimeStamp;
            if (timePassed <= TimeSpan.FromMinutes(30)) return user;
        
            _tokenService.DeleteOtp(user.Username);
            throw new SessionExpiredException();
        });
        
        return await getUserTask;
    }
    
    public async Task<Contact> GetUserEmailAsync(string username)
    {
        var getEmailTask = Task.Run(() =>
        {
            var contact = _ldapService.GetUserContact(username);
            return contact;
        });

        return await getEmailTask;
    }
}