using PasswordChanger.Web.Models;

namespace PasswordChanger.Web.Abstractions.Services;

public interface IRecaptchaService
{
    Task<RecaptchaAssessment> AssessRecaptchaAsync(string recaptchaToken, string action,
        CancellationToken cancellationToken);

    Task<RecaptchaAssessment> AssessRecaptchaAsync(string recaptchaToken, string action,
        string? userId, CancellationToken cancellationToken);

    Task AnnotateWrongPasswordAsync(RecaptchaAssessment assessment, CancellationToken cancellationToken);

    Task AnnotateCorrect2FaAsync(RecaptchaAssessment assessment, CancellationToken cancellationToken);

    Task AnnotateWrongPasswordResetTokenAsync(RecaptchaAssessment assessment, CancellationToken cancellationToken);
}
