using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosPetCore.Domain.RepoContracts;
using WhosPetCore.Domain.ServiceContracts;

namespace WhosPetCore.Domain.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;

        public UserRoleService(IUserRoleRepository userRoleRepository)
        {
            _userRoleRepository = userRoleRepository;
        }

        public Task<List<string>> GetUserRoles(string userId, string connectionString)
        {
            return _userRoleRepository.GetUserRoles(userId);
        }

        public Task<List<string>> GetRoles(List<string> userRoles, string connectionString)
        {
            return _userRoleRepository.GetRoles(userRoles);
        }
    }

}
