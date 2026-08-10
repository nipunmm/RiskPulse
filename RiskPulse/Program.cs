using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RiskPulse.Data;
using RiskPulse.Services;
using RiskPulse.Services.AccessControlService;
using RiskPulse.Services.LoginService;

var builder = WebApplication.CreateBuilder(args);

// Retrieve connection string and register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<AdAuthenticationService>();
builder.Services.AddScoped<DbAuthorizationService>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<RolesService>();
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
    options.AddPolicy("Permission:Dashboard", policy => policy.RequireClaim("Permission", "Dashboard"));
    options.AddPolicy("Permission:Submissions", policy => policy.RequireClaim("Permission", "Submissions"));
    options.AddPolicy("Permission:Assessment Control", policy => policy.RequireClaim("Permission", "Assessment Control"));
    options.AddPolicy("Permission:Form Builder", policy => policy.RequireClaim("Permission", "Form Builder"));
    options.AddPolicy("Permission:Roles", policy => policy.RequireClaim("Permission", "Roles"));
    options.AddPolicy("Permission:Users", policy => policy.RequireClaim("Permission", "Users"));
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
