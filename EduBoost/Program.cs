using EduBoost.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
var builder = WebApplication.CreateBuilder(args);

var baseConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontro la cadena de conexion 'DefaultConnection'.");
var dbPassword = builder.Configuration["SqlServer:Password"]
    ?? Environment.GetEnvironmentVariable("EDUBOOST_DB_PASSWORD");
var finalConnection = BuildConnectionString(baseConnection, dbPassword);

builder.Services.AddDbContext<EduBoostContext>(options =>
    options.UseSqlServer(finalConnection));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.AccessDeniedPath = "/Usuarios/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EduBoostContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

static string BuildConnectionString(string baseConnection, string? password)
{
    var builder = new SqlConnectionStringBuilder(baseConnection);
    if (!builder.IntegratedSecurity)
    {
        if (!string.IsNullOrWhiteSpace(password))
        {
            builder.Password = password;
        }
        else if (builder.Password == "{DB_PASSWORD}")
        {
            builder.Password = string.Empty;
        }
    }

    return builder.ConnectionString;
}
