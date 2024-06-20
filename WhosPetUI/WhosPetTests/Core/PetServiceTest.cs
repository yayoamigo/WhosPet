using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Threading.Tasks;
using WhosPetCore.Services;
using WhosPetCore.Domain.RepoContracts;
using WhosPetCore.Helpers;
using WhosPetCore.DTO.Incoming.Pets;
using WhosPetCore.DTO.Outgoing;
using WhosPetCore.Domain.Entities;
using System.Collections.Generic;
using System.Security.Claims;

namespace WhosPetTests.Core
{
    public class PetServiceTests
    {
        private readonly Mock<IPetsRepository> _petsRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ILogger<PetService>> _loggerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly PetService _petService;

        public PetServiceTests()
        {
            _petsRepositoryMock = new Mock<IPetsRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<PetService>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _mapperMock = new Mock<IMapper>();

            _petService = new PetService(
                _petsRepositoryMock.Object,
                _userRepositoryMock.Object,
                _loggerMock.Object,
                _httpContextAccessorMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task AddPet_ShouldReturnSuccessResponse_WhenPetIsAdded()
        {
            // Arrange
            var addPetDTO = new AddPetDTO { Name = "Buddy", Type = "Dog" };
            var userEmail = "test@example.com";
            var user = new UserProfile { Email = userEmail };
            var pet = new Pets { Name = "Buddy", Type = "Dog", Owner = user };
            var petId = 1;
            var petDetails = new PetDetails { Id = petId, Name = "Buddy", Type = "Dog" };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Email, userEmail)
            }, "mock"));

            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipal);
            _userRepositoryMock.Setup(x => x.GetUserbyEmail(userEmail)).ReturnsAsync(user);
            _mapperMock.Setup(m => m.Map<Pets>(addPetDTO)).Returns(pet);
            _petsRepositoryMock.Setup(x => x.AddPet(pet)).ReturnsAsync(petId);
            _mapperMock.Setup(m => m.Map<List<PetDetails>>(It.IsAny<List<Pets>>())).Returns(new List<PetDetails> { petDetails });

            // Act
            var result = await _petService.AddPet(addPetDTO);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Pet added successfully", result.Message);
            Assert.Single(result.Details);
            Assert.Equal(petDetails.Name, result.Details[0].Name);
        }

        [Fact]
        public async Task DeletePet_ShouldReturnTrue_WhenPetIsDeleted()
        {
            // Arrange
            var petId = 1;
            var userEmail = "test@example.com";
            var user = new UserProfile { Email = userEmail };
            var pets = new List<Pets> { new Pets { Id = petId, Owner = user } };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Email, userEmail)
            }, "mock"));

            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipal);
            _userRepositoryMock.Setup(x => x.GetUserbyEmail(userEmail)).ReturnsAsync(user);
            _petsRepositoryMock.Setup(x => x.GetUserPets(user)).ReturnsAsync(pets);
            _petsRepositoryMock.Setup(x => x.DeletePet(petId)).ReturnsAsync(true);

            // Act
            var result = await _petService.DeletePet(petId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetPetByType_ShouldReturnSuccessResponse_WhenPetsFound()
        {
            // Arrange
            var petType = "Dog";
            var pets = new List<Pets> { new Pets { Name = "Buddy", Type = "Dog" } };
            var petDetails = new List<PetDetails> { new PetDetails { Name = "Buddy", Type = "Dog" } };

            _petsRepositoryMock.Setup(x => x.GetPetByType(petType)).ReturnsAsync(pets);
            _mapperMock.Setup(m => m.Map<List<PetDetails>>(pets)).Returns(petDetails);

            // Act
            var result = await _petService.GetPetByType(petType);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Pets found", result.Message);
            Assert.Single(result.Details);
            Assert.Equal(petDetails[0].Name, result.Details[0].Name);
        }

        [Fact]
        public async Task GetPetsByCity_ShouldReturnSuccessResponse_WhenPetsFound()
        {
            // Arrange
            var city = "New York";
            var pets = new List<Pets> { new Pets { Name = "Buddy", Breed = "Chichihua" } };
            var petDetails = new List<PetDetails> { new PetDetails { Name = "Buddy", Breed = "Chichihua" } };

            _petsRepositoryMock.Setup(x => x.GetPetsByCity(city)).ReturnsAsync(pets);
            _mapperMock.Setup(m => m.Map<List<PetDetails>>(pets)).Returns(petDetails);

            // Act
            var result = await _petService.GetPetsByCity(city);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Pets found", result.Message);
            Assert.Single(result.Details);
            Assert.Equal(petDetails[0].Name, result.Details[0].Name);
        }

        [Fact]
        public async Task GetPetsByShelter_ShouldReturnSuccessResponse_WhenPetsFound()
        {
            // Arrange
            var shelterName = "Happy Paws Shelter";
            var pets = new List<Pets> { new Pets { Name = "Buddy", UserId = "Happy Paws Shelter" } };
            var petDetails = new List<PetDetails> { new PetDetails { Name = "Buddy" } };

            _petsRepositoryMock.Setup(x => x.GetPetsByShelter(shelterName)).ReturnsAsync(pets);
            _mapperMock.Setup(m => m.Map<List<PetDetails>>(pets)).Returns(petDetails);

            // Act
            var result = await _petService.GetPetsByShelter(shelterName);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Pets found", result.Message);
            Assert.Single(result.Details);
            Assert.Equal(petDetails[0].Name, result.Details[0].Name);
        }

        [Fact]
        public async Task GetUserPets_ShouldReturnSuccessResponse_WhenPetsFound()
        {
            // Arrange
            var userEmail = "test@example.com";
            var user = new UserProfile { Email = userEmail };
            var pets = new List<Pets> { new Pets { Name = "Buddy", Owner = user } };
            var petDetails = new List<PetDetails> { new PetDetails { Name = "Buddy" } };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Email, userEmail)
            }, "mock"));

            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipal);
            _userRepositoryMock.Setup(x => x.GetUserbyEmail(userEmail)).ReturnsAsync(user);
            _petsRepositoryMock.Setup(x => x.GetUserPets(user)).ReturnsAsync(pets);
            _mapperMock.Setup(m => m.Map<List<PetDetails>>(pets)).Returns(petDetails);

            // Act
            var result = await _petService.GetUserPets();

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Pets found", result.Message);
            Assert.Single(result.Details);
            Assert.Equal(petDetails[0].Name, result.Details[0].Name);
        }

        [Fact]
        public async Task UpdatePet_ShouldReturnSuccessResponse_WhenPetIsUpdated()
        {
            // Arrange
            var updatePetDTO = new UpdatePetDTO { Id = 1, Name = "Buddy", Type = "Dog" };
            var pet = new Pets { Id = 1, Name = "Buddy", Type = "Dog" };
            var petDetails = new List<PetDetails> { new PetDetails { Id = 1, Name = "Buddy", Type = "Dog" } };

            _mapperMock.Setup(m => m.Map<Pets>(updatePetDTO)).Returns(pet);
            _petsRepositoryMock.Setup(x => x.UpdatePet(pet)).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<List<PetDetails>>(It.IsAny<List<Pets>>())).Returns(petDetails);

            // Act
            var result = await _petService.UpdatePet(updatePetDTO);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Pet updated successfully", result.Message);
            Assert.Single(result.Details);
            Assert.Equal(petDetails[0].Name, result.Details[0].Name);
        }
    }
}
