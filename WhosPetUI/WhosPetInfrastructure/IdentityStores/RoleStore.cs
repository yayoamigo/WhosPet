using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhosPetCore.Domain.Indentity;

namespace WhosPetAuth.IdentityStores
{
    public class RoleStore : IRoleStore<ApplicationRole>, IQueryableRoleStore<ApplicationRole>
    {
        private readonly string _connectionString;
        private readonly List<ApplicationRole> _roles;

        public RoleStore(ConnectionStringOptions options)
        {
            _connectionString = options.ConnectionString;
            _roles = new List<ApplicationRole>();

            LoadRolesFromDatabase();
        }

        private void LoadRolesFromDatabase()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT * FROM AspNetRoles", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var role = new ApplicationRole
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString(),
                                NormalizedName = reader["NormalizedName"].ToString()
                            };

                            _roles.Add(role);
                        }
                    }
                }
            }
        }

        public IQueryable<ApplicationRole> Roles => _roles.AsQueryable();

        public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                role.Id = Guid.NewGuid().ToString();
                role.NormalizedName = role.Name.ToUpper();

                using (var command = new SqlCommand("INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES (@Id, @Name, @NormalizedName)", connection))
                {
                    command.Parameters.AddWithValue("@Id", role.Id);
                    command.Parameters.AddWithValue("@Name", role.Name);
                    command.Parameters.AddWithValue("@NormalizedName", role.NormalizedName);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }

            return IdentityResult.Success;
        }

        public Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
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

        public Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
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

        public Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
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
                            return Task.FromResult<ApplicationRole>(null);
                        }

                        var role = new ApplicationRole
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

        public Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
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
                            return Task.FromResult<ApplicationRole>(null);
                        }

                        var role = new ApplicationRole
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

        public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public Task SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public void Dispose() { }
    }
}
