using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Microsoft.Extensions.Options;
using PasswordChanger.Web.Abstractions.Services;
using PasswordChanger.Web.Exceptions;
using PasswordChanger.Web.Models;
using PasswordChanger.Web.Models.Configuration;

namespace PasswordChanger.Web.Services;

public class LdapService : ILdapService
{
    private const string Email = "mail";
    
    private const string FirstName = "givenName";

    private const string LastName = "sn";
        
    private readonly IOptions<DomainConfiguration> _domainConfiguration;

    public LdapService(IOptions<DomainConfiguration> domainConfiguration)
    {
        _domainConfiguration = domainConfiguration;
    }

    public Contact GetUserContact(string username)
    {
        var de = TryGetDirectoryEntry(username);

        if (de.Properties[Email].Value is null)
        {
            throw new UserWithoutEmailException();
        }
        
        var email = de.Properties[Email].Value!.ToString();
        var firstName = de.Properties[FirstName].Value?.ToString();
        var lastName = de.Properties[LastName].Value?.ToString();
        
        var contact = new Contact
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };
        
        return contact;
    }

    public void ChangePassword(string username, string newPassword)
    {
        var user = TryGetUser(username);
        
        user.SetPassword(newPassword);
        user.Save();
    }

    private UserPrincipal TryGetUser(string username)
    {
        using var context = new PrincipalContext(ContextType.Domain, _domainConfiguration.Value.Endpoint);
        
        var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
        
        if (user is null)
        {
            throw new UserNotFoundException();
        }
        
        return user;
    }
    
    private DirectoryEntry TryGetDirectoryEntry(string username)
    {
        var user = TryGetUser(username);

        if (user.GetUnderlyingObject() is not DirectoryEntry de)
        {
            throw new UserNotFoundException();
        }

        return de;
    }
}
