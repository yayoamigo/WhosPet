using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosPetCore.Domain.Entities.Auth;

namespace WhosPetCore.Domain.RepoContracts
{
    public interface IRefreshTokenRepository
    {
        Task AddRefreshToken(RefreshToken refreshToken);
    }
}
