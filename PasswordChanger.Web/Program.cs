using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using PasswordChanger.Web.Abstractions.Services;
using PasswordChanger.Web.Consts;
using PasswordChanger.Web.Helpers;
using PasswordChanger.Web.Models.Configuration;
using PasswordChanger.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddEventLog();
    });
    
builder.Services.Configure<DomainConfiguration>(builder.Configuration.GetSection("Domain"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailOptions"));

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Keys"))
    .SetApplicationName("PasswordChangerApp")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(30));

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});
builder.Services.AddMvc().AddViewLocalization();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7282, listenOptions =>
    {
        listenOptions.UseHttps(HttpsHelper.GetKeyFromContainer("00b44a99082d840e5e"));
    });
});

builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IEmailService, EmailService>();

builder.Services.AddTransient<ILdapService, LdapService>();
builder.Services.AddTransient<IUserService, UserService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var supportedCultures = new CultureInfo[] { new(Idioms.EnUs), new(Idioms.ItIt) };
var requestLocalizationOptions = new RequestLocalizationOptions
{
    RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()],
    ApplyCurrentCultureToResponseHeaders = true,
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    DefaultRequestCulture = new RequestCulture(Idioms.EnUs)
};

app.UseRequestLocalization(requestLocalizationOptions);

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.RunAsync();
