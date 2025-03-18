using System.Text;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using PasswordChanger.Web.Abstractions.Services;
using PasswordChanger.Web.Consts;
using PasswordChanger.Web.Models;
using PasswordChanger.Web.Models.Configuration;

namespace PasswordChanger.Web.Services;

public class EmailService : IEmailService, IDisposable
{
    private readonly SmtpClient _client;
    
    private readonly IOptions<EmailOptions> _emailOptions;
    
    private readonly IStringLocalizer<EmailService> _localizer;

    public EmailService(IOptions<EmailOptions> emailOptions, IStringLocalizer<EmailService> localizer)
    {
        _emailOptions = emailOptions;
        _localizer = localizer;

        _client = new SmtpClient();
        _client.Connect(_emailOptions.Value.Smtp.Host, _emailOptions.Value.Smtp.Port, _emailOptions.Value.Smtp.UseSsl);
        _client.Authenticate(_emailOptions.Value.Smtp.Username, _emailOptions.Value.Smtp.Password);
    }

    public async Task SendOtpAsync(OtpSendRequest otpSendRequest)
    {
        var message = new MimeMessage();

        var senderAddress = new MailboxAddress(_emailOptions.Value.From.Name, _emailOptions.Value.From.Address);
        message.From.Add(senderAddress);

        var receiverName = $"{otpSendRequest.User.FirstName} {otpSendRequest.User.LastName}".Trim();
        var receiverAddress = new MailboxAddress(receiverName, otpSendRequest.User.Email);
        message.To.Add(receiverAddress);

        message.Subject = _localizer[nameof(Resources.Services.EmailService.EmailSubject)].ToString();

        var bodyText = _localizer[nameof(Resources.Services.EmailService.EmailBody), otpSendRequest.Otp].ToString();
        var body = new TextPart("plain") { Text = bodyText };
        message.Body = body;

        await _client.SendAsync(message);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _client.Dispose();
    }
}