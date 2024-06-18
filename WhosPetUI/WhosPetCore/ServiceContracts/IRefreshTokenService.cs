using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosPetCore.Domain.Entities.Auth;

namespace WhosPetCore.ServiceContracts
{
    public interface IRefreshTokenService
    {
        Task AddRefreshToken(RefreshToken refreshToken, string connectionString);
    }
}
