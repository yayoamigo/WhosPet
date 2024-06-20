using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WhosPetAuth;
using WhosPetAuth.IdentityStores;
using WhosPetCore.Domain.Indentity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WhosPetAuth.Configuration;
using WhosPetCore.Domain.RepoContracts;
using WhosPetInfrastructure.Repositories;
using System.Security.Claims;
using WhosPetCore.Domain.Entities;
using System.Data.SqlClient;
using WhosPetCore.Domain.ServiceContracts;
using WhosPetCore.Domain.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("WhosPet");

builder.Services.AddSingleton(new ConnectionStringOptions { ConnectionString = connectionString });

builder.Services.AddScoped<IUserStore<ApplicationUser>, UserStore>();
builder.Services.AddScoped<IRoleStore<ApplicationRole>, RoleStore>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 0;
})
    .AddUserStore<UserStore>()
    .AddRoleStore<RoleStore>()
    .AddRoleManager<RoleManager<ApplicationRole>>();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"))
    .SetApplicationName("WhosPetApp");

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
    ClockSkew = TimeSpan.Zero
};

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

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var roles = new[] { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }
    }

    var ownerEmail = "yayon@example.com";
    var owner = await userManager.FindByEmailAsync(ownerEmail);

    if (owner == null)
    {
        var user = new ApplicationUser
        {
            Email = ownerEmail,
            UserName = ownerEmail,
            EmailConfirmed = true,
        };

        string password = "string";
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");

            AddUserProfileToDatabase(connectionString, user.Email, user.UserName, "David", "Guayaquil", "Calle 123");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };
            await userManager.AddClaimsAsync(user, claims);
        }
        else
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error creating user: {error.Description}");
            }
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

void AddUserProfileToDatabase(string connectionString, string email, string name, string surname, string city, string address)
{
    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();

        var query = "INSERT INTO UserProfile (Email, Name, Surname, City, Address) VALUES (@Email, @Name, @Surname, @City, @Address)";

        using (var command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Surname", surname);
            command.Parameters.AddWithValue("@City", city);
            command.Parameters.AddWithValue("@Address", address);

            command.ExecuteNonQuery();
        }
    }
}
