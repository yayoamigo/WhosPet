using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosPetAuth;
using WhosPetCore.Domain.RepoContracts;

namespace WhosPetInfrastructure.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly string _connectionString;

        public UserRoleRepository(ConnectionStringOptions options)
        {
            _connectionString = options.ConnectionString;
        }

        public async Task<List<string>> GetUserRoles(string userId)
        {
            var userRoles = new List<string>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT RoleId FROM AspNetUserRoles WHERE UserId = @UserId";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            userRoles.Add(reader["RoleId"].ToString());
                        }
                    }
                }
            }

            return userRoles;
        }

        public async Task<List<string>> GetRoles(List<string> userRoles)
        {
            var roles = new List<string>();

            if (userRoles.Count == 0)
            {
                return roles;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT Name FROM AspNetRoles WHERE Id IN (@RoleIds)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RoleIds", string.Join(",", userRoles));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            roles.Add(reader["Name"].ToString());
                        }
                    }
                }
            }

            return roles;
        }
    }

}
