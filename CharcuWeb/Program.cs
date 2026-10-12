using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Infrastructure;
using TradicioCarnica.Services;

var builder = WebApplication.CreateBuilder(args);

// Localización
builder.Services.AddLocalization();

var supportedCultures = new[]
{
    new CultureInfo("es"),
    new CultureInfo("ca")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es"),   // si el navegador no es es/ca
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

// Orden: 1) ?culture=ca  2) cookie (para un futuro selector)  3) idioma del navegador
localizationOptions.RequestCultureProviders.Clear();
localizationOptions.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
localizationOptions.RequestCultureProviders.Add(new CookieRequestCultureProvider());
localizationOptions.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());

// Base de datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Política de Admin
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// Razor Pages + protección de la carpeta Admin y del registro de usuarios
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Register", "AdminOnly");
})
.AddMvcOptions(o => o.ModelBinderProviders.Insert(0, new InvariantDecimalModelBinderProvider()));

// Servicios propios
builder.Services.AddScoped<StockService>();

var app = builder.Build();

app.UseRequestLocalization(localizationOptions);

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // HSTS: 30 días por defecto; ver https://aka.ms/aspnetcore-hsts
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();