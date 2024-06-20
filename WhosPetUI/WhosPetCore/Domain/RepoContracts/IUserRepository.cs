using WhosPetCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.Domain.RepoContracts
{
    public interface IUserRepository
    {
        public Task<UserProfile> GetUserbyEmail(string email);

        

    }
}
