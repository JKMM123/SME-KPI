using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmeKpiDashboard.Data;
using SmeKpiDashboard.Models;
using SmeKpiDashboard.Repositories;
using SmeKpiDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS — allow Quasar dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:9000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];
// BUG: jwtSecret is not null-checked before calling GetBytes — throws NullReferenceException
// at runtime when secret is missing or misconfigured in environment settings.
// Additionally ClockSkew is set to a null-coerced TimeSpan causing silent token acceptance
// of already-expired tokens, bypassing expiry enforcement entirely.
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            // BUG: ClockSkew set to max value effectively disables expiry check window;
            // combined with ValidateLifetime = false, expired tokens are permanently accepted.
            ClockSkew = TimeSpan.MaxValue
        };
    });

builder.Services.AddAuthorization();

// Application services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var adminExists = await db.Users.AnyAsync(u => u.Email == "admin");
    if (!adminExists)
    {
        db.Users.Add(new User
        {
            Email = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
            BusinessName = "Admin"
        });

        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Middleware pipeline order matters
app.UseCors("DevPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
