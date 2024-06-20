using WhosPetCore.Domain.RepoContracts;
using WhosPetCore.Helpers;
using WhosPetCore.Domain.Entities;
using WhosPetCore.DTO.Incoming.Pets;
using WhosPetCore.DTO.Outgoing;
using WhosPetCore.ServiceContracts.ReportContracts;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WhosPetCore.Services
{
    public class LostReportService : IAddLostReport, IGetLostReports, IUpdatePetReport
    {
        private readonly ILostReportRepository _lostPetReportRepository;
        private readonly IPetsRepository _petsRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<LostReportService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public LostReportService(
            ILostReportRepository lostPetReportRepository,
            IPetsRepository petsRepository,
            IUserRepository userRepository,
            ILogger<LostReportService> logger,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            INotificationRepository notificationRepository)
        {
            _lostPetReportRepository = lostPetReportRepository;
            _petsRepository = petsRepository;
            _userRepository = userRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _notificationRepository = notificationRepository;
        }

        public async Task<bool> CreateLostPetReportAsync(LostReportDTO reportDto)
        {
            var userEmail = _httpContextAccessor.HttpContext.User.GetEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogError("User email not found");
                return false;
            }
            _logger.LogInformation($"Email retrieved: {userEmail}");

            var user = await _userRepository.GetUserbyEmail(userEmail);

            if (user == null)
            {
                _logger.LogError("User or user profile not found");
                return false;
            }

            var userPets = await _petsRepository.GetUserPets(user);

            var reportedPet = userPets.FirstOrDefault(p => p.Name == reportDto.PetName);
            if (reportedPet == null)
            {
                _logger.LogError("Pet not found");
                return false;
            }

            var lostPetReport = _mapper.Map<LostPetReport>(reportDto);
            lostPetReport.Pet = reportedPet;
            lostPetReport.UserProfile = user;

            var result = await _lostPetReportRepository.AddLostPetReportAsync(lostPetReport);
    

            var notification = new Notification
            {
                
                Message = $"Help us, {reportedPet.Name} got lost in {reportedPet.City}",
                UserProfile = user,
                Timestamp = DateTime.Now,
                IsRead = false
            };

            await _notificationRepository.AddNotificationAsync(notification);

            return result > 0;
        }

        public async Task<IEnumerable<LostPetResponseDTO>> GetLostPetReportByCity(string City)
        {
            if(string.IsNullOrEmpty(City))
            {
                _logger.LogError("City is null or empty");
                return null;
            }

            var result = await _lostPetReportRepository.GetLostPetReportByCityAsync(City);

            if (result == null || !result.Any())
            {
                _logger.LogError("No reports found");
                return null;
            }

            var response = _mapper.Map<IEnumerable<LostPetResponseDTO>>(result);

            return response;
        }

        public async Task<bool> FoundLostPet(int lostReportID)
        {
            if (lostReportID <= 0)
            {
                _logger.LogError("Invalid id");
                return false;
            }

            var result = await _lostPetReportRepository.FoundLostPet(lostReportID);

            return result;
        }
    }
}
