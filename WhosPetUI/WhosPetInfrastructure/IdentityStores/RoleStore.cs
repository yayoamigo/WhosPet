using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WhosPetAuth.IdentityStores
{

    public class RoleStore : IRoleStore<IdentityRole>
    {
        private readonly string _connectionString;

        public RoleStore(ConnectionStringOptions options)
        {
            _connectionString = options.ConnectionString;
        }

        public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand("INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES (@Id, @Name, @NormalizedName)", connection))
                    {
                        command.Parameters.AddWithValue("@Id", role.Id);
                        command.Parameters.AddWithValue("@Name", role.Name);
                        command.Parameters.AddWithValue("@NormalizedName", role.NormalizedName);

                        command.ExecuteNonQuery();
                    }
                }

                return Task.FromResult(IdentityResult.Success);
            }


       public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("UPDATE AspNetRoles SET Name = @Name, NormalizedName = @NormalizedName WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", role.Id);
                    command.Parameters.AddWithValue("@Name", role.Name);
                    command.Parameters.AddWithValue("@NormalizedName", role.NormalizedName);

                    command.ExecuteNonQuery();
                }
            }

            return Task.FromResult(IdentityResult.Success);
           
        }

        public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("DELETE FROM AspNetRoles WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", role.Id);

                    command.ExecuteNonQuery();
                }
            }

            return Task.FromResult(IdentityResult.Success);
           
        }

        public Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT * FROM AspNetRoles WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", roleId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return Task.FromResult<IdentityRole>(null);
                        }

                        var role = new IdentityRole
                        {
                            Id = reader["Id"].ToString(),
                            Name = reader["Name"].ToString(),
                            NormalizedName = reader["NormalizedName"].ToString()
                        };

                        return Task.FromResult(role);
                    }
                }
            }
           
        }

        public Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT * FROM AspNetRoles WHERE NormalizedName = @NormalizedName", connection))
                {
                    command.Parameters.AddWithValue("@NormalizedName", normalizedRoleName);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return Task.FromResult<IdentityRole>(null);
                        }

                        var role = new IdentityRole
                        {
                            Id = reader["Id"].ToString(),
                            Name = reader["Name"].ToString(),
                            NormalizedName = reader["NormalizedName"].ToString()
                        };

                        return Task.FromResult(role);
                    }
                }
            }
           
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
        {        

            return Task.FromResult(true);

        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            // Do nothing. In this simple example, the normalized name is generated from the role name.

            return Task.FromResult(true);
        }

        public void Dispose() { }
    }
}
