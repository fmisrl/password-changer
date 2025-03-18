using PasswordChanger.Web.Models;

namespace PasswordChanger.Web.Abstractions.Services;

public interface IEmailService
{
    Task SendOtpAsync(OtpSendRequest otpSendRequest);
}