using AlertaPatitasAPIUI.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosPetCore.Domain.Entities.Auth;
using WhosPetCore.Domain.Indentity;
using WhosPetCore.Domain.ServiceContracts;
using WhosPetCore.DTO.Incoming.Auth;

namespace WhosPetCore.Helpers
{
    public class JwtHelper
    {
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly string _connectionString;
        private readonly IUserRoleService _roleService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;

        public JwtHelper(TokenValidationParameters tokenValidationParameters, string connectionString, IUserRoleService roleService, IRefreshTokenService refreshTokenService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config)
        {
            _tokenValidationParameters = tokenValidationParameters;
            _connectionString = connectionString;
            _roleService = roleService;
            _refreshTokenService = refreshTokenService;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        public virtual async Task<AuthResponse> VeryfyAndGenerateToken(TokenRequestDTO token)
        {

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                _tokenValidationParameters.ValidateLifetime = false;

                var tokenInVerification = jwtTokenHandler.ValidateToken(token.Token, _tokenValidationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (!result)
                    {
                        return new AuthResponse()
                        {
                            Result = false,
                            Errors = new List<string> { "Invalid token" }
                        };
                    }
                }

                var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

                if (expiryDate > DateTime.UtcNow)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string> { "This token has not expired yet" }
                    };
                }

                var storedToken = await GetRefreshTokenAsync(token.RefreshToken);

                if (storedToken == null)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string> { "This refresh token doesn't exist" }
                    };
                }

                if (DateTime.UtcNow > storedToken.ExpireDate)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string> { "This refresh token has expired" }
                    };
                }

                if (storedToken.IsUsed)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string> { "This refresh token has been used" }
                    };
                }

                if (storedToken.IsRevoked)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string> { "This refresh token has been revoked" }
                    };
                }

                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if (storedToken.jwtId != jti)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string> { "This refresh token doesn't match this JWT" }
                    };
                }

                await RevokeRefreshTokenAsync(storedToken);

                var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
                if (user != null)
                {
                    var jwt = new JwtTokenGenerator(_roleService, _refreshTokenService);
                    return await jwt.GenerateJwtToken(user, _config, _connectionString);
                }

                return new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string> { "User not found" }
                };
            }
            catch (Exception)
            {
                return new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string> { "Server Error" }
                };
            }
        }
        public virtual  DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTime;
        }
        public virtual async Task<RefreshToken> GetRefreshTokenAsync(string refreshToken)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT * FROM RefreshToken WHERE Token = @Token";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Token", refreshToken);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new RefreshToken
                            {
                                jwtId = reader["JwtId"].ToString(),
                                IsUsed = (bool)reader["IsUsed"],
                                IsRevoked = (bool)reader["IsRevoked"],
                                UserId = reader["UserId"].ToString(),
                                CreatedDate = (DateTime)reader["CreatedDate"],
                                ExpireDate = (DateTime)reader["ExpireDate"],
                                Token = reader["Token"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }
        public virtual async Task RevokeRefreshTokenAsync(RefreshToken refreshToken)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "DELETE FROM RefreshToken WHERE Token = @Token";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Token", refreshToken.Token);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }

}
