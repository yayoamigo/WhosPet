using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WhosPetAuth;
using WhosPetAuth.IdentityStores;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WhosPetAuth.Configuration;
using WhosPetCore.Domain.Indentity;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using WhosPetCore.ServiceContracts;
using WhosPetCore.Services;
using WhosPetCore.Domain.RepoContracts;
using WhosPetInfrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("WhosPet");

// Register the connection string as a configuration option
builder.Services.AddSingleton(new ConnectionStringOptions { ConnectionString = connectionString });

// Register the UserStore and RoleStore with the DI container
builder.Services.AddScoped<IUserStore<ApplicationUser>, UserStore>();
builder.Services.AddScoped<IRoleStore<IdentityRole>, RoleStore>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();

// Configure Identity without default token providers
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Disable password requirements
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 1; // Minimum length of 1
    options.Password.RequiredUniqueChars = 0; // No unique character requirement
})
    .AddUserStore<UserStore>()
    .AddRoleStore<RoleStore>();

// Configure Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys")) // Specify a directory to store keys
    .SetApplicationName("WhosPetApp"); // Set a unique application name

// Configure JWT
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));
byte[] key = Encoding.ASCII.GetBytes(builder.Configuration["JwtConfig:Secret"]);

var tokenValidationParameters = new TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = false,
    ValidateAudience = false,
    RequireExpirationTime = true,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero // Optional: Reduce the allowed clock skew for token expiration
};

// Register TokenValidationParameters
builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwt =>
{
    jwt.SaveToken = true;
    jwt.TokenValidationParameters = tokenValidationParameters;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); // Ensure UseAuthentication is before UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
