using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Services;
using RiskPulse.Services.Administration;
using RiskPulse.Services.Assessment;
using RiskPulse.Services.Login;
using RiskPulse.Services.Templates;

var builder = WebApplication.CreateBuilder(args);

// Retrieve connection string and register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Persist/parse enums (e.g. SaqStatus "Active") as strings in JSON bodies.
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddScoped<AdAuthenticationService>();
builder.Services.AddScoped<DbAuthorizationService>();
builder.Services.AddScoped<LoginOrchestratorService>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<RolesService>();
builder.Services.AddScoped<UnitsService>();
builder.Services.AddScoped<SaqTemplatesService>();
builder.Services.AddScoped<KriTemplatesService>();
builder.Services.AddScoped<AssessmentService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy($"Permission:{PermissionCatalog.Dashboard}", policy => policy.RequireClaim("Permission", PermissionCatalog.Dashboard));
    options.AddPolicy($"Permission:{PermissionCatalog.Submissions}", policy => policy.RequireClaim("Permission", PermissionCatalog.Submissions));
    options.AddPolicy($"Permission:{PermissionCatalog.Assessment}", policy => policy.RequireClaim("Permission", PermissionCatalog.Assessment));
    options.AddPolicy($"Permission:{PermissionCatalog.Roles}", policy => policy.RequireClaim("Permission", PermissionCatalog.Roles));
    options.AddPolicy($"Permission:{PermissionCatalog.Users}", policy => policy.RequireClaim("Permission", PermissionCatalog.Users));
    options.AddPolicy($"Permission:{PermissionCatalog.Units}", policy => policy.RequireClaim("Permission", PermissionCatalog.Units));
    options.AddPolicy($"Permission:{PermissionCatalog.Saq}", policy => policy.RequireClaim("Permission", PermissionCatalog.Saq));
    options.AddPolicy($"Permission:{PermissionCatalog.Kri}", policy => policy.RequireClaim("Permission", PermissionCatalog.Kri));
    options.AddPolicy($"Permission:{PermissionCatalog.RiskRegister}", policy => policy.RequireClaim("Permission", PermissionCatalog.RiskRegister));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePages();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
