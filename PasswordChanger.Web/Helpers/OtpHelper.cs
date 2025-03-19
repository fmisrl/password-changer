using System.Security.Cryptography;

namespace PasswordChanger.Web.Helpers;

internal static class OtpHelper
{
    public static string GenerateOtp()
    {
        var random = RandomNumberGenerator.GetInt32(100000, 999999);
        return random.ToString();
    }
}
