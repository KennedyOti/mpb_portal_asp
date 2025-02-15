using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MPB_PORTAL_WEB_APP.Data;
using MPB_PORTAL_WEB_APP.Models;
using MPB_PORTAL_WEB_APP.Seeders; // Import the Seeder class
using MPB_PORTAL_WEB_APP.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEmailSender, EmailSender>(); // Register EmailSender

// Add services to the container
builder.Services.AddControllersWithViews();



// Configure Database Connection
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Identity
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false; // Allow passwords without special characters
    options.Password.RequiredLength = 8; // Minimum password length
    options.Password.RequireUppercase = false; // Allow passwords without uppercase letters
    options.Password.RequireDigit = true; // Require at least one digit
    options.User.RequireUniqueEmail = true; // Ensure emails are unique
    options.SignIn.RequireConfirmedEmail = false; // Set to true if email confirmation is required
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders(); // For email confirmation and password reset

// Configure cookie settings (optional but recommended)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Session timeout
    options.LoginPath = "/Account/Login"; // Redirect to login page if unauthorized
    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect to access denied page
    options.SlidingExpiration = true; // Reset expiration time on activity
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed initial roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<Users>>();

        // Seed Roles
        await IdentitySeeder.SeedRoles(roleManager);

        // Seed Admin User
        await IdentitySeeder.SeedAdminUser(userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
