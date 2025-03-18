using PasswordChanger.Web.Models;

namespace PasswordChanger.Web.Abstractions.Services;

public interface ILdapService
{
    Contact GetUserContact(string username);
    
    void ChangePassword(string username, string newPassword);
}