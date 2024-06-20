using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WhosPetCore.Domain.Entities;
using WhosPetCore.Domain.Indentity;
using WhosPetCore.Domain.RepoContracts;
using Microsoft.AspNetCore.Identity;
using WhosPetAuth;

namespace WhosPetInfrastructure.Repos
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(
            ConnectionStringOptions options,
            UserManager<ApplicationUser> userManager
            )
        {
            _connectionString = options.ConnectionString;
            _userManager = userManager;
        }


        public async Task<UserProfile> GetUserbyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM UserProfiles WHERE Email = @Email", connection);
                command.Parameters.AddWithValue("@Email", email);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapReaderToUserProfile(reader);
                    }
                }
            }
            return null;
        }
        private UserProfile MapReaderToUserProfile(SqlDataReader reader)
        {
            return new UserProfile
            {
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Surname = reader.GetString(reader.GetOrdinal("Surname")),
                City = reader.GetString(reader.GetOrdinal("City")),
                Address = reader.GetString(reader.GetOrdinal("Address"))
                
            };
        }
    }
}
