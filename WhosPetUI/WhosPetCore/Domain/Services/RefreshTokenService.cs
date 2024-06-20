using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosPetCore.Domain.Entities.Auth;
using WhosPetCore.Domain.RepoContracts;
using WhosPetCore.Domain.ServiceContracts;

namespace WhosPetCore.Domain.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public Task AddRefreshToken(RefreshToken refreshToken, string connectionString)
        {
            return _refreshTokenRepository.AddRefreshToken(refreshToken);
        }
    }
}
