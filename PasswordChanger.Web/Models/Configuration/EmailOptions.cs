namespace PasswordChanger.Web.Models.Configuration;

public class EmailOptions
{
    public Sender From { get; set; } = null!;

    public SmtpOptions Smtp { get; set; } = null!;

    public class Sender
    {
        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;
    }

    public class SmtpOptions
    {
        public string Host { get; set; } = null!;

        public int Port { get; set; }

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public bool UseSsl { get; set; }
    }
}