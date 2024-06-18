using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.Domain.RepoContracts
{
    public interface IUserRoleRepository
    {
        Task<List<string>> GetUserRoles(string userId);
        Task<List<string>> GetRoles(List<string> userRoles);
    }
}
