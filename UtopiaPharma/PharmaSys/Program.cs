using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaSys.Data;
using PharmaSys.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Razor Pages + Identity
// =======================
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToFolder("/Identity/Account");
});

// =======================
// DbContext
// =======================
builder.Services.AddDbContext<PharmaSysDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// =======================
// Identity
// =======================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<PharmaSysDbContext>()
.AddDefaultTokenProviders();

// =======================
// Cookies
// =======================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// =======================
// ✅ Session (PANIER)
// =======================
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// =======================
// Services
// =======================
builder.Services.AddScoped<AlertService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();

// =======================
// Authorization globale
// =======================
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// =======================
// ✅ CONFIGURATION DE LA CULTURE (CORRECTION MONTANTS)
// =======================
var cultureInfo = new CultureInfo("fr-MA");
cultureInfo.NumberFormat.NumberDecimalSeparator = "."; // ✅ Force le point pour les calculs
cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
cultureInfo.NumberFormat.CurrencySymbol = "DH";

// Applique la configuration au pipeline pour que le serveur lise bien les points envoyés par le navigateur
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureInfo),
    SupportedCultures = new List<CultureInfo> { cultureInfo },
    SupportedUICultures = new List<CultureInfo> { cultureInfo }
});

// =======================
// Pipeline HTTP
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

//  IMPORTANT: Session AVANT Auth
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// =======================
// SEED DATA
// =======================
using (var scope = app.Services.CreateScope())
{
    await SeedData.SeedAsync(scope.ServiceProvider);
}

app.Run();