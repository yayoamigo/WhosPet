using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WhosPetCore.Domain.Entities.Auth;
using WhosPetCore.Domain.Indentity;
using WhosPetCore.Domain.ServiceContracts;




namespace AlertaPatitasAPIUI.Helpers
{
    public class JwtTokenGenerator
    {
        private readonly IUserRoleService _userRoleService;
        private readonly IRefreshTokenService _refreshTokenService;

        public JwtTokenGenerator(IUserRoleService userRoleService, IRefreshTokenService refreshTokenService)
        {
            _userRoleService = userRoleService;
            _refreshTokenService = refreshTokenService;
        }

        public virtual async Task<AuthResponse> GenerateJwtToken(ApplicationUser user, IConfiguration config, string connectionString)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(config["JwtConfig:Secret"]);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.Integer64),
        };

            var userRoles = await _userRoleService.GetUserRoles(user.Id, connectionString);
            var roles = await _userRoleService.GetRoles(userRoles, connectionString);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TimeSpan.Parse(config["JwtConfig:ExpireTime"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var tokenString = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                jwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                CreatedDate = DateTime.UtcNow,
                ExpireDate = DateTime.UtcNow.AddMonths(1),
                Token = RandomStringGenerator(25) + Guid.NewGuid()
            };

            await _refreshTokenService.AddRefreshToken(refreshToken, connectionString);

            return new AuthResponse()
            {
                Token = tokenString,
                RefreshToken = refreshToken.Token,
                Expiration = token.ValidTo,
                Result = true,
            };
        }

        public static string RandomStringGenerator(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

}
