using WhosPetCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.ServiceContracts.UserContracts
{
    public interface IGetUsersService
    {
        Task<UserProfile> GetUserbyEmail(string email);


    }
}
