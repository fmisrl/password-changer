using System.Collections.Concurrent;
using PasswordChanger.Web.Abstractions.Services;
using PasswordChanger.Web.Helpers;

namespace PasswordChanger.Web.Services;

public class TokenService : ITokenService
{
    private readonly ConcurrentDictionary<string, string> _otpStorage = new();
    
    public bool TryValidateOtp(string username, string otp)
    {
        if (!_otpStorage.ContainsKey(username)) return false;

        var success = _otpStorage.TryGetValue(username, out var foundOtp);
        
        return success && foundOtp == otp;
    }

    public string GetOtp(string username)
    {
        var otp = OtpHelper.GenerateOtp();

        if (!_otpStorage.TryAdd(username, otp))
        {
            _otpStorage[username] = otp;
        }
        
        return otp;
    }

    public void DeleteOtp(string username)
    {
        _otpStorage.TryRemove(username, out _);
    }
}