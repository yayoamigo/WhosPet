using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosPetAuth;
using WhosPetCore.Domain.Entities.Auth;
using WhosPetCore.Domain.RepoContracts;

namespace WhosPetInfrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly string _connectionString;

        public RefreshTokenRepository(ConnectionStringOptions options)
        {
            _connectionString = options.ConnectionString;
        }

        public async Task AddRefreshToken(RefreshToken refreshToken)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "INSERT INTO RefreshTokens (JwtId, IsUsed, IsRevoked, UserId, CreatedDate, ExpireDate, Token) VALUES (@JwtId, @IsUsed, @IsRevoked, @UserId, @CreatedDate, @ExpireDate, @Token)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@JwtId", refreshToken.jwtId);
                    command.Parameters.AddWithValue("@IsUsed", refreshToken.IsUsed);
                    command.Parameters.AddWithValue("@IsRevoked", refreshToken.IsRevoked);
                    command.Parameters.AddWithValue("@UserId", refreshToken.UserId);
                    command.Parameters.AddWithValue("@CreatedDate", refreshToken.CreatedDate);
                    command.Parameters.AddWithValue("@ExpireDate", refreshToken.ExpireDate);
                    command.Parameters.AddWithValue("@Token", refreshToken.Token);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
