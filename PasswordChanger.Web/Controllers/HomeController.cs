using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using PasswordChanger.Web.Abstractions.Services;
using PasswordChanger.Web.Consts;
using PasswordChanger.Web.Exceptions;
using PasswordChanger.Web.Models;

namespace PasswordChanger.Web.Controllers;

public class HomeController : Controller
{
    private readonly IUserService _userService;
    
    private readonly IStringLocalizer<HomeController> _localizer;
    
    private readonly ILogger<HomeController> _logger;

    public HomeController(IUserService userService,
        ILogger<HomeController> logger, IStringLocalizer<HomeController> localizer)
    {
        _userService = userService;
        _logger = logger;
        _localizer = localizer;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromForm] User user)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, _localizer[nameof(Resources.HomeController.UsernameNotValid)]);
            return View(user);
        }


        try
        {
            var otp = await _userService.FindUserAndGetSessionTokenAsync(user.Username);
            
            var contact = await _userService.GetUserEmailAsync(user.Username);
            
            if (contact.Email != null)
            {
                HttpContext.Session.SetString(Session.SessionKeyEmail, contact.Email);
            }
            
            SaveUserInTheSession(user.Username, otp);
            
            return RedirectToAction("OtpValidation", "Home");
        }
        catch (UserNotFoundException)
        {
            ModelState.AddModelError(string.Empty, _localizer[nameof(Resources.HomeController.UserNotFound)]);
        }
        catch (UserWithoutEmailException)
        {
            ModelState.AddModelError(string.Empty, _localizer[nameof(Resources.HomeController.UserWithoutEmail)]);
        }
        catch (Exception e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
        }
        return View();
    }

    public IActionResult OtpValidation()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> OtpValidation([FromForm(Name = "otp")] string otp)
    {
        if (string.IsNullOrEmpty(otp))
        {
            ModelState.AddModelError(string.Empty, _localizer[nameof(Resources.HomeController.InvalidOtp)]);
            return View();
        }

        User user;
        try
        {
            user = await GetUserAndCheckSessionValidationAsync();
        }
        catch (SessionExpiredException)
        {
            HttpContext.Session.SetString(Session.SessionKeyError, _localizer[nameof(Resources.HomeController.SessionExpired)]);
            return RedirectToAction("Index");
        }

        try
        {
            var isValid = await _userService.ValidateOtpAsync(user.Username, otp);
            
            if (isValid)
            {
                return RedirectToAction("PwChange");
            }

            ModelState.AddModelError(string.Empty, _localizer[nameof(Resources.HomeController.WrongOtp)]);
        }
        catch (Exception e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
        }
        return View();
    }

    public IActionResult PwChange()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> PwChange([FromForm(Name = "password")] string password,
        [FromForm(Name = "confirmPassword")] string confirmPassword)
    {
        if (!string.Equals(password, confirmPassword))
        {
            ModelState.AddModelError(string.Empty, _localizer[nameof(Resources.HomeController.PasswordsDoNotMatch)]);
            return View();
        }

        User user;
        
        try
        {
            user = await GetUserAndCheckSessionValidationAsync();
        }
        catch (SessionExpiredException)
        {
            HttpContext.Session.SetString(Session.SessionKeyError, _localizer[nameof(Resources.HomeController.SessionExpired)]);
            return RedirectToAction("Index");
        }

        try
        {
            await _userService.ChangeUserPasswordAsync(user.Username, password);
            
            return RedirectToAction("Confirm");
        }
        catch (UserNotFoundException)
        {
            ModelState.AddModelError(string.Empty, _localizer[nameof(Resources.HomeController.UserNotFound)]);
            return View();
        }
    }

    public IActionResult Confirm()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<User> GetUserAndCheckSessionValidationAsync()
    {
        var userJson = HttpContext.Session.GetString(Session.SessionKeyUsername);
        
        try
        {
            var user = await _userService.GetUserAndCheckSessionValidationAsync(userJson);
            
            return user;
        }
        catch (SessionExpiredException)
        {
            HttpContext.Session.Remove(Session.SessionKeyUsername);
            HttpContext.Session.Remove(Session.SessionKeyEmail);
            
            throw new SessionExpiredException();
        }
    }
    
    private void SaveUserInTheSession(string username, string otp)
    {
        var user = new User
        {
            Username = username,
            Otp = otp,
            OtpTimeStamp = DateTime.UtcNow
        };
        
        var userJson = JsonSerializer.Serialize(user);
        
        HttpContext.Session.SetString(Session.SessionKeyUsername, userJson);
    }
}