using WhosPetCore.Domain.RepoContracts;
using WhosPetCore.ServiceContracts.Notifications;
using WhosPetCore.ServiceContracts.PetContracts;
using WhosPetCore.ServiceContracts.ReportContracts;
using WhosPetCore.ServiceContracts.UserContracts;
using WhosPetCore.Services;
using WhosPetInfrastructure.Repos;
using WhosPetUI.ExHandlreMiddleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using WhosPetCore.Domain.Entities;
using WhosPetCore.Domain.Indentity;
using WhosPetUI.Configuration;
using WhosPetAuth;
using WhosPetAuth.IdentityStores;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("http://localhost:5174", "http://localhost:5173", "http://localhost:5175")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});


builder.Services.AddControllers();
builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("WhosPet");

builder.Services.AddSingleton(new ConnectionStringOptions { ConnectionString = connectionString });

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));
byte[] key = Encoding.ASCII.GetBytes(builder.Configuration["JwtConfig:Secret"]);
var TokenValidationParameters = new TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = false,
    ValidateAudience = false,
    RequireExpirationTime = true,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero 
};
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwt =>
{
    jwt.SaveToken = true;
    jwt.TokenValidationParameters = TokenValidationParameters;
    
});

#region Dependency Injection
builder.Services.AddSingleton(TokenValidationParameters);
builder.Services.AddScoped<IPetsRepository, PetsRepository>();
builder.Services.AddScoped<IAddPetService, PetService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGetUsersService, UserService>();
builder.Services.AddScoped<IGetPetService, PetService>();
builder.Services.AddScoped<IUpdatePetService, PetService>();
builder.Services.AddScoped<IDeletePetService, PetService>();
builder.Services.AddScoped<ILostReportRepository, LostReportRepository>();
builder.Services.AddScoped<IAddLostReport, LostReportService>();
builder.Services.AddScoped<IGetLostReports, LostReportService>();
builder.Services.AddScoped<IUpdatePetReport, LostReportService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

#endregion
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddUserStore<UserStore>()
    .AddRoleStore<RoleStore>()
    .AddRoleManager<RoleManager<ApplicationRole>>();

builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



// Middleware
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandler>();

app.MapControllers();

app.Run();