using CS322_PZ_AnteaPrimorac5157.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ganss.Xss;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using CS322_PZ_AnteaPrimorac5157.Repositories;
using CS322_PZ_AnteaPrimorac5157.Services;
using Microsoft.Extensions.Logging;
using CS322_PZ_AnteaPrimorac5157.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<IConfessionRepository, ConfessionRepository>();
builder.Services.AddScoped<IConfessionService, ConfessionService>();

// Konfiguracija ASP.NET Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Isključi registraciju
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;

    // Postavke zaključavanja
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.AccessDeniedPath = "/Home/Index";
});


builder.Services.AddSingleton<HtmlSanitizer>(serviceProvider =>
{
    var sanitizer = new HtmlSanitizer();
    sanitizer.AllowedTags.Clear();
    sanitizer.AllowedTags.Add("b");
    sanitizer.AllowedTags.Add("i");
    sanitizer.AllowedTags.Add("u");
    sanitizer.AllowedTags.Add("s");
    return sanitizer;
});

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages(options =>
{
    // Zahtijevaj autentifikaciju za sve Identity stranice
    options.Conventions.AuthorizeAreaFolder("Identity", "/");

    // Dozvoli pristup login stranici
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Logout");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DisableManagementPages", policy =>
        policy.RequireAssertion(context => false)); // Zabrani account manage stranice
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed za admina (osigurava da uvijek postoji bar jedan)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var adminEmail = "admin@admin.com";

        if (!userManager.Users.Any())
        {
            var admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create admin user");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the admin user.");
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseIdentityPagesRestriction();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.UseSession();

app.Run();
