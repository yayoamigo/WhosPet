using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.ServiceContracts
{
    public interface IUserRoleService
    {
        Task<List<string>> GetUserRoles(string userId, string connectionString);
        Task<List<string>> GetRoles(List<string> userRoles, string connectionString);
    }
}
