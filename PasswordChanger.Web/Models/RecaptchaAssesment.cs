namespace PasswordChanger.Web.Models;

public class RecaptchaAssessment
{
    public bool Success { get; set; }

    public bool HighRisk { get; set; }

    public string Id { get; set; } = null!;
}
