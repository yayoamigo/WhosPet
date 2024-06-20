using WhosPetCore.Domain.Entities;
using WhosPetCore.Domain.RepoContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WhosPetCore.Helpers;
using WhosPetCore.DTO.Outgoing;
using AutoMapper;
using WhosPetCore.DTO.Incoming.Pets;
using WhosPetCore.ServiceContracts.PetContracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WhosPetCore.Services
{
    public class PetService : IAddPetService, IGetPetService, IUpdatePetService, IDeletePetService
    {
        private readonly IPetsRepository _petsRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PetService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public PetService(
            IPetsRepository petsRepository,
            IUserRepository userRepository,
            ILogger<PetService> logger,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _petsRepository = petsRepository;
            _userRepository = userRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<PetResponseDTO> AddPet(AddPetDTO addPetDTO)
        {
            var userEmail = _httpContextAccessor.HttpContext.User.GetEmail();

            _logger.LogInformation($"Email retrieved: {userEmail}");

            var user = await _userRepository.GetUserbyEmail(userEmail);

            if (user == null)
            {
                return new PetResponseDTO
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            UserProfile ownerProfile = user;

            Pets pet = _mapper.Map<Pets>(addPetDTO);
            pet.Owner = ownerProfile;

            var result = await _petsRepository.AddPet(pet);

            if (result <= 0)
            {
                return new PetResponseDTO
                {
                    Success = false,
                    Message = "Failed to add pet"
                };
            }

            pet.Id = result;

            var petDetails = _mapper.Map<List<PetDetails>>(new List<Pets> { pet });

            return new PetResponseDTO
            {
                Success = true,
                Message = "Pet added successfully",
                Details = petDetails
            };
        }

        public async Task<bool> DeletePet(int id)
        {
            var userEmail = _httpContextAccessor.HttpContext.User.GetEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogError("No valid user is making the request");
                return false;
            }

            var user = await _userRepository.GetUserbyEmail(userEmail);

            if (user == null)
            {
                return false;
            }

            var pets = await _petsRepository.GetUserPets(user);

            if (!pets.Any(p => p.Id == id))
            {
                return false;
            }

            if (id <= 0)
            {
                _logger.LogError("Invalid pet id");
                return false;
            }

            var result = await _petsRepository.DeletePet(id);

            return result;
        }

        public async Task<PetResponseDTO> GetPetByType(string type)
        {
            var pets = await _petsRepository.GetPetByType(type);
            if (pets == null)
            {
                return new PetResponseDTO
                {
                    Success = false,
                    Message = "No pets found for the specified type"
                };
            }

            var petDetails = _mapper.Map<List<PetDetails>>(pets);

            return new PetResponseDTO
            {
                Success = true,
                Message = "Pets found",
                Details = petDetails
            };
        }

        public async Task<PetResponseDTO> GetPetsByCity(string city)
        {
            if (string.IsNullOrEmpty(city))
            {
                _logger.LogError("City cannot be null or empty");
                return null;
            }

            var pets = await _petsRepository.GetPetsByCity(city);

            if (pets == null)
            {
                return new PetResponseDTO
                {
                    Success = false,
                    Message = "No pets found for the specified city"
                };
            }

            var petDetails = _mapper.Map<List<PetDetails>>(pets);

            return new PetResponseDTO
            {
                Success = true,
                Message = "Pets found",
                Details = petDetails
            };
        }

        public async Task<PetResponseDTO> GetPetsByShelter(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                _logger.LogError("Shelter name cannot be null or empty");
                return null;
            }

            var pets = await _petsRepository.GetPetsByShelter(name);

            if (pets == null)
            {
                return new PetResponseDTO
                {
                    Success = false,
                    Message = "No pets found for the specified shelter"
                };
            }

            var petDetails = _mapper.Map<List<PetDetails>>(pets);

            return new PetResponseDTO
            {
                Success = true,
                Message = "Pets found",
                Details = petDetails
            };
        }

        public async Task<PetResponseDTO> GetUserPets()
        {
            var userEmail = _httpContextAccessor.HttpContext.User.GetEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogError("No valid user is making the request");
                return null;
            }

            var user = await _userRepository.GetUserbyEmail(userEmail);

            if (user == null)
            {
                return null;
            }

            var pets = await _petsRepository.GetUserPets(user);

            if (pets == null || pets.Count == 0)
            {
                return new PetResponseDTO
                {
                    Success = false,
                    Message = "No pets found for the user"
                };
            }

            var petDetails = _mapper.Map<List<PetDetails>>(pets);

            return new PetResponseDTO
            {
                Success = true,
                Message = "Pets found",
                Details = petDetails
            };
        }

        public async Task<PetResponseDTO> UpdatePet(UpdatePetDTO pet)
        {
            if (pet == null)
            {
                _logger.LogError("Invalid pet information");
                return null;
            }

            var petToUpdate = _mapper.Map<Pets>(pet);

            if (petToUpdate == null)
            {
                _logger.LogError("Invalid pet information");
                return null;
            }

            var result = await _petsRepository.UpdatePet(petToUpdate);

            if (!result)
            {
                return new PetResponseDTO
                {
                    Success = false,
                    Message = "Failed to update pet"
                };
            }

            var petDetails = _mapper.Map<List<PetDetails>>(new List<Pets> { petToUpdate });

            return new PetResponseDTO
            {
                Success = true,
                Message = "Pet updated successfully",
                Details = petDetails
            };
        }
    }
}
