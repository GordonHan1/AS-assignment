using Microsoft.AspNetCore.Identity;
using WebApplication1.Helpers;
using WebApplication1.Model;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.Validators;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Configure CryptoHelper with the encryption key from appsettings.json
CryptoHelper.Configure(configuration);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<AuthDbContext>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AuthDbContext>().AddPasswordValidator<PasswordHistoryValidator>(); ;
builder.Services.AddHttpClient();         // needed for IHttpClientFactory
builder.Services.AddTransient<GoogleCaptchaService>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredUniqueChars = 1;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});
builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options
=>
{
	options.Cookie.Name = "MyCookieAuth";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
builder.Services.ConfigureApplicationCookie(options =>
{
    // Absolute expiration (e.g., 30 minutes)
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

    // Sliding expiration will refresh the cookie if the user is active
    options.SlidingExpiration = true;

    // The login path if the cookie is expired
    options.LoginPath = "/Login";
});
builder.Services.AddAuthorization(options =>
{
options.AddPolicy("MustBelongToHRDepartment",
policy => policy.RequireClaim("Department", "HR"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/InternalServerError");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
