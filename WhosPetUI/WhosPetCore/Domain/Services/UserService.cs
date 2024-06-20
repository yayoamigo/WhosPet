using WhosPetCore.Domain.Entities;
using WhosPetCore.Helpers;
using WhosPetCore.Domain.RepoContracts;
using WhosPetCore.DTO.Incoming.Users;
using WhosPetCore.DTO.Outgoing;
using WhosPetCore.ServiceContracts.UserContracts;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WhosPetCore.Services
{
    public class UserService : IGetUsersService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<UserService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<UserProfile> GetUserbyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var user = await _userRepository.GetUserbyEmail(email);

            if (user == null)
            {
                _logger.LogError("User not found");
                return null;
            }
            return user;
        }

        
    }
}
