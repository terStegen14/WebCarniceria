using TradicioCarnica.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Localización
builder.Services.AddLocalization();

var supportedCultures = new[]
{
    new CultureInfo("es"),
    new CultureInfo("ca")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};
localizationOptions.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
localizationOptions.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Política de Admin
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// Razor Pages + protecci�n de carpeta Admin
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
});

var app = builder.Build();
//using var scope = app.Services.CreateScope();
//var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

//Crear rol Admin si no existe
//if (!await roleManager.RoleExistsAsync("Admin"))
//{
//    await roleManager.CreateAsync(new IdentityRole("Admin"));
//}

//Crear usuario superadmin si no existe
//var adminEmail = "superadmin@superadmin.com";
//var adminUser = await userManager.FindByEmailAsync(adminEmail);
//if (adminUser == null)
//{
//    adminUser = new IdentityUser
//    {
//        UserName = "SuperAdmin",
//        Email = adminEmail,
//        EmailConfirmed = true
//    };
//    var result = await userManager.CreateAsync(adminUser, "SuperAdmin123!");

//    if (!result.Succeeded)
//    {
//        foreach (var error in result.Errors)
//        {
//            Console.WriteLine(error.Description);
//        }
//        throw new Exception("Error creando el usuario superadmin");
//    }
//}

////A�adir rol
//if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
//{
//    await userManager.AddToRoleAsync(adminUser, "Admin");
//}

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
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
