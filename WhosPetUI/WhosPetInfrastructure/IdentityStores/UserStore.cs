using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WhosPetCore.Domain.Entities;
using WhosPetCore.Domain.Enums;
using WhosPetCore.Domain.Indentity;
using System.Security.Claims;

namespace WhosPetAuth.IdentityStores
{
    public class UserStore : IUserStore<ApplicationUser>,
                             IUserEmailStore<ApplicationUser>,
                             IUserPasswordStore<ApplicationUser>,
                             IUserLoginStore<ApplicationUser>,
                             IUserClaimStore<ApplicationUser>,
                             IUserRoleStore<ApplicationUser>
    {
        private readonly string _connectionString;

        public UserStore(ConnectionStringOptions options)
        {
            _connectionString = options.ConnectionString;
        }

        public async Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("INSERT INTO AspNetUserLogins (LoginProvider, ProviderKey, ProviderDisplayName, UserId) VALUES (@LoginProvider, @ProviderKey, @ProviderDisplayName, @UserId)", connection))
                {
                    command.Parameters.AddWithValue("@LoginProvider", login.LoginProvider);
                    command.Parameters.AddWithValue("@ProviderKey", login.ProviderKey);
                    command.Parameters.AddWithValue("@ProviderDisplayName", login.ProviderDisplayName);
                    command.Parameters.AddWithValue("@UserId", user.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, PasswordHash) VALUES (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @PasswordHash)", connection))
                {
                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.Parameters.AddWithValue("@UserName", user.UserName);
                    command.Parameters.AddWithValue("@NormalizedUserName", user.NormalizedUserName);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@NormalizedEmail", user.NormalizedEmail);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                if (user.Profile != null)
                {
                    using (var profileCommand = new SqlCommand("INSERT INTO UserProfiles (Email, Name, Surname, City, Address, UserId) VALUES (@Email, @Name, @Surname, @City, @Address, @UserId)", connection))
                    {
                        profileCommand.Parameters.AddWithValue("@Email", user.Profile.Email);
                        profileCommand.Parameters.AddWithValue("@Name", user.Profile.Name);
                        profileCommand.Parameters.AddWithValue("@Surname", user.Profile.Surname);
                        profileCommand.Parameters.AddWithValue("@City", user.Profile.City); 
                        profileCommand.Parameters.AddWithValue("@Address", user.Profile.Address);
                        profileCommand.Parameters.AddWithValue("@UserId", user.Id); 

                        await profileCommand.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("DELETE FROM AspNetUsers WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", user.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                // Delete UserProfile
                using (var profileCommand = new SqlCommand("DELETE FROM UserProfiles WHERE Email = @Email", connection))
                {
                    profileCommand.Parameters.AddWithValue("@Email", user.Email);

                    await profileCommand.ExecuteNonQueryAsync(cancellationToken);
                }
            }

            return IdentityResult.Success;
        }

        public void Dispose()
        {
            // Dispose any resources if needed
        }

        public async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                ApplicationUser user = null;

                using (var command = new SqlCommand("SELECT * FROM AspNetUsers WHERE NormalizedEmail = @NormalizedEmail", connection))
                {
                    command.Parameters.AddWithValue("@NormalizedEmail", normalizedEmail);

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            user = new ApplicationUser
                            {
                                Id = reader["Id"].ToString(),
                                UserName = reader["UserName"].ToString(),
                                NormalizedUserName = reader["NormalizedUserName"].ToString(),
                                Email = reader["Email"].ToString(),
                                NormalizedEmail = reader["NormalizedEmail"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString()
                            };
                        }
                    }
                }

                if (user != null)
                {
                    user.Profile = await FindUserProfileByEmailAsync(user.Email, cancellationToken, connection);
                }

                return user;
            }
        }


        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("SELECT * FROM AspNetUsers WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            var user = new ApplicationUser
                            {
                                Id = reader["Id"].ToString(),
                                UserName = reader["UserName"].ToString(),
                                NormalizedUserName = reader["NormalizedUserName"].ToString(),
                                Email = reader["Email"].ToString(),
                                NormalizedEmail = reader["NormalizedEmail"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString()
                            };

                            // Retrieve UserProfile
                            user.Profile = await FindUserProfileByEmailAsync(user.Email, cancellationToken, connection);
                            return user;
                        }
                    }
                }
            }

            return null;
        }

        public async Task<ApplicationUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("SELECT * FROM AspNetUserLogins WHERE LoginProvider = @LoginProvider AND ProviderKey = @ProviderKey", connection))
                {
                    command.Parameters.AddWithValue("@LoginProvider", loginProvider);
                    command.Parameters.AddWithValue("@ProviderKey", providerKey);

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            var userId = reader["UserId"].ToString();

                            return await FindByIdAsync(userId, cancellationToken);
                        }
                    }
                }
            }

            return null;
        }

        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("SELECT * FROM AspNetUsers WHERE NormalizedUserName = @NormalizedUserName", connection))
                {
                    command.Parameters.AddWithValue("@NormalizedUserName", normalizedUserName);

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            var user = new ApplicationUser
                            {
                                Id = reader["Id"].ToString(),
                                UserName = reader["UserName"].ToString(),
                                NormalizedUserName = reader["NormalizedUserName"].ToString(),
                                Email = reader["Email"].ToString(),
                                NormalizedEmail = reader["NormalizedEmail"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString()
                            };

                            // Retrieve UserProfile
                            user.Profile = await FindUserProfileByEmailAsync(user.Email, cancellationToken, connection);
                            return user;
                        }
                    }
                }
            }

            return null;
        }

        public Task<string> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(true);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var logins = new List<UserLoginInfo>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT * FROM AspNetUserLogins WHERE UserId = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var login = new UserLoginInfo(reader["LoginProvider"].ToString(), reader["ProviderKey"].ToString(), reader["ProviderDisplayName"].ToString());
                            logins.Add(login);
                        }
                    }
                }
            }

            return Task.FromResult<IList<UserLoginInfo>>(logins);
        }

        public Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.UserName);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.PasswordHash != null);
        }

        public async Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("DELETE FROM AspNetUserLogins WHERE UserId = @UserId AND LoginProvider = @LoginProvider AND ProviderKey = @ProviderKey", connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);
                    command.Parameters.AddWithValue("@LoginProvider", loginProvider);
                    command.Parameters.AddWithValue("@ProviderKey", providerKey);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        public async Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            user.Email = email;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("UPDATE AspNetUsers SET Email = @Email WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Id", user.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            // In this example, we are not storing email confirmation, so we can return a completed task
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public async Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            user.PasswordHash = passwordHash;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("UPDATE AspNetUsers SET PasswordHash = @PasswordHash WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    command.Parameters.AddWithValue("@Id", user.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("UPDATE AspNetUsers SET UserName = @UserName, NormalizedUserName = @NormalizedUserName, Email = @Email, NormalizedEmail = @NormalizedEmail, PasswordHash = @PasswordHash WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@UserName", user.UserName);
                    command.Parameters.AddWithValue("@NormalizedUserName", user.NormalizedUserName);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@NormalizedEmail", user.NormalizedEmail);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@Id", user.Id);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                // Update UserProfile
                if (user.Profile != null)
                {
                    using (var profileCommand = new SqlCommand("UPDATE UserProfiles SET Name = @Name, Surname = @Surname, City = @City, Address = @Address, UserID = @UserId WHERE Email = @Email", connection))
                    {
                        profileCommand.Parameters.AddWithValue("@Name", user.Profile.Name);
                        profileCommand.Parameters.AddWithValue("@Surname", user.Profile.Surname);
                        profileCommand.Parameters.AddWithValue("@City", user.Profile.City);
                        profileCommand.Parameters.AddWithValue("@Address", user.Profile.Address);
                        profileCommand.Parameters.AddWithValue("@Email", user.Profile.Email);
                        profileCommand.Parameters.AddWithValue("@UserId", user.Id);

                        await profileCommand.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }

            return IdentityResult.Success;
        }

        public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                foreach (var claim in claims)
                {
                    using (var command = new SqlCommand("INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue) VALUES (@UserId, @ClaimType, @ClaimValue)", connection))
                    {
                        command.Parameters.AddWithValue("@UserId", user.Id);
                        command.Parameters.AddWithValue("@ClaimType", claim.Type);
                        command.Parameters.AddWithValue("@ClaimValue", claim.Value);

                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }
        }

        public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var claims = new List<Claim>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("SELECT * FROM AspNetUserClaims WHERE UserId = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            claims.Add(new Claim(reader["ClaimType"].ToString(), reader["ClaimValue"].ToString()));
                        }
                    }
                }
            }

            return claims;
        }

        public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                foreach (var claim in claims)
                {
                    using (var command = new SqlCommand("DELETE FROM AspNetUserClaims WHERE UserId = @UserId AND ClaimType = @ClaimType AND ClaimValue = @ClaimValue", connection))
                    {
                        command.Parameters.AddWithValue("@UserId", user.Id);
                        command.Parameters.AddWithValue("@ClaimType", claim.Type);
                        command.Parameters.AddWithValue("@ClaimValue", claim.Value);

                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }
        }

        public async Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand("UPDATE AspNetUserClaims SET ClaimType = @NewClaimType, ClaimValue = @NewClaimValue WHERE UserId = @UserId AND ClaimType = @ClaimType AND ClaimValue = @ClaimValue", connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);
                    command.Parameters.AddWithValue("@ClaimType", claim.Type);
                    command.Parameters.AddWithValue("@ClaimValue", claim.Value);
                    command.Parameters.AddWithValue("@NewClaimType", newClaim.Type);
                    command.Parameters.AddWithValue("@NewClaimValue", newClaim.Value);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        private async Task<UserProfile> FindUserProfileByEmailAsync(string email, CancellationToken cancellationToken, SqlConnection connection)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var command = new SqlCommand("SELECT * FROM UserProfiles WHERE Email = @Email", connection))
            {
                command.Parameters.AddWithValue("@Email", email);

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        return new UserProfile
                        {
                            Email = reader["Email"].ToString(),
                            Name = reader["Name"].ToString(),
                            Surname = reader["Surname"].ToString(),
                            City = reader["City"].ToString(),
                            Address = reader["Address"].ToString()
                        };
                    }
                }
            }

            return null;
        }


        public async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var users = new List<ApplicationUser>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new SqlCommand(@"
            SELECT u.Id, u.UserName, u.NormalizedUserName, u.Email, u.NormalizedEmail, u.PasswordHash
            FROM AspNetUsers u
            INNER JOIN AspNetUserClaims c ON u.Id = c.UserId
            WHERE c.ClaimType = @ClaimType AND c.ClaimValue = @ClaimValue", connection))
                {
                    command.Parameters.AddWithValue("@ClaimType", claim.Type);
                    command.Parameters.AddWithValue("@ClaimValue", claim.Value);

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var user = new ApplicationUser
                            {
                                Id = reader["Id"].ToString(),
                                UserName = reader["UserName"].ToString(),
                                NormalizedUserName = reader["NormalizedUserName"].ToString(),
                                Email = reader["Email"].ToString(),
                                NormalizedEmail = reader["NormalizedEmail"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString()
                            };

                            // Retrieve UserProfile
                            user.Profile = await FindUserProfileByEmailAsync(user.Email, cancellationToken, connection);
                            users.Add(user);
                        }
                    }
                }
            }

            return users;
        }

        public Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using(var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using(var command = new SqlCommand("INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)", connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);
                    command.Parameters.AddWithValue("@RoleId", roleName);

                    command.ExecuteNonQuery();
                }
            }

            return Task.CompletedTask;

        }

        public Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            
            cancellationToken.ThrowIfCancellationRequested();

            using(var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using(var command = new SqlCommand("DELETE FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);
                    command.Parameters.AddWithValue("@RoleId", roleName);

                    command.ExecuteNonQuery();
                }
            }

            return Task.CompletedTask;
        }

        public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            
            cancellationToken.ThrowIfCancellationRequested();

            var roles = new List<string>();

            using(var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using(var command = new SqlCommand(@"
                SELECT r.Name
                FROM AspNetRoles r
                INNER JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
                WHERE ur.UserId = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);

                    using(var reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            roles.Add(reader["Name"].ToString());
                        }
                    }
                }
            }

            return Task.FromResult<IList<string>>(roles);
        }

        public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
           
            cancellationToken.ThrowIfCancellationRequested();

            using(var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using(var command = new SqlCommand(@"
                SELECT COUNT(*)
                FROM AspNetRoles r
                INNER JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
                WHERE ur.UserId = @UserId AND r.Name = @RoleName", connection))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);
                    command.Parameters.AddWithValue("@RoleName", roleName);

                    var count = (int)command.ExecuteScalar();

                    return Task.FromResult(count > 0);
                }
            }
        }

        public Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
           
            cancellationToken.ThrowIfCancellationRequested();

            var users = new List<ApplicationUser>();

            using(var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using(var command = new SqlCommand(@"
                SELECT u.Id, u.UserName, u.NormalizedUserName, u.Email, u.NormalizedEmail, u.PasswordHash
                FROM AspNetUsers u
                INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                INNER JOIN AspNetRoles r ON r.Id = ur.RoleId
                WHERE r.Name = @RoleName", connection))
                {
                    command.Parameters.AddWithValue("@RoleName", roleName);

                    using(var reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            var user = new ApplicationUser
                            {
                                Id = reader["Id"].ToString(),
                                UserName = reader["UserName"].ToString(),
                                NormalizedUserName = reader["NormalizedUserName"].ToString(),
                                Email = reader["Email"].ToString(),
                                NormalizedEmail = reader["NormalizedEmail"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString()
                            };

                            // Retrieve UserProfile
                            user.Profile = FindUserProfileByEmailAsync(user.Email, cancellationToken, connection).Result;
                            users.Add(user);
                        }
                    }
                }
            }

            return Task.FromResult<IList<ApplicationUser>>(users);
        }
    }
}
