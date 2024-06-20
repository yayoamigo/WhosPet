using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Threading.Tasks;
using WhosPetCore.Services;
using WhosPetCore.Domain.RepoContracts;
using WhosPetCore.DTO.Outgoing;
using WhosPetCore.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Security.Claims;
using WhosPetCore.DTO.Incoming.Pets;

namespace WhosPetTests.Core
{
    public class LostReportServiceTests
    {
        private readonly Mock<ILostReportRepository> _lostReportRepositoryMock;
        private readonly Mock<IPetsRepository> _petsRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<INotificationRepository> _notificationRepositoryMock;
        private readonly Mock<ILogger<LostReportService>> _loggerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly LostReportService _lostReportService;

        public LostReportServiceTests()
        {
            _lostReportRepositoryMock = new Mock<ILostReportRepository>();
            _petsRepositoryMock = new Mock<IPetsRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _notificationRepositoryMock = new Mock<INotificationRepository>();
            _loggerMock = new Mock<ILogger<LostReportService>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _mapperMock = new Mock<IMapper>();

            _lostReportService = new LostReportService(
                _lostReportRepositoryMock.Object,
                _petsRepositoryMock.Object,
                _userRepositoryMock.Object,
                _loggerMock.Object,
                _httpContextAccessorMock.Object,
                _mapperMock.Object,
                _notificationRepositoryMock.Object
            );
        }

        [Fact]
        public async Task CreateLostPetReportAsync_ShouldReturnFalse_WhenUserEmailIsNull()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { }, "mock"));
            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipal);

            // Act
            var result = await _lostReportService.CreateLostPetReportAsync(new LostReportDTO());

            // Assert
            Assert.False(result);
            VerifyLogger(LogLevel.Error, "User email not found");
        }

        [Fact]
        public async Task CreateLostPetReportAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            // Arrange
            var email = "test@example.com";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, email)
            }, "mock"));

            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipal);
            _userRepositoryMock.Setup(x => x.GetUserbyEmail(email)).ReturnsAsync((UserProfile)null);

            // Act
            var result = await _lostReportService.CreateLostPetReportAsync(new LostReportDTO());

            // Assert
            Assert.False(result);
            VerifyLogger(LogLevel.Error, "User or user profile not found");
        }

        [Fact]
        public async Task CreateLostPetReportAsync_ShouldReturnFalse_WhenPetNotFound()
        {
            // Arrange
            var email = "test@example.com";
            var user = new UserProfile { Email = email };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, email)
            }, "mock"));

            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipal);
            _userRepositoryMock.Setup(x => x.GetUserbyEmail(email)).ReturnsAsync(user);
            _petsRepositoryMock.Setup(x => x.GetUserPets(user)).ReturnsAsync(new List<Pets>());

            var reportDto = new LostReportDTO { PetName = "Fluffy" };

            // Act
            var result = await _lostReportService.CreateLostPetReportAsync(reportDto);

            // Assert
            Assert.False(result);
            VerifyLogger(LogLevel.Error, "Pet not found");
        }

        [Fact]
        public async Task CreateLostPetReportAsync_ShouldReturnTrue_WhenReportIsCreatedSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var user = new UserProfile { Email = email };
            var pet = new Pets { Name = "Fluffy", City = "SampleCity" };
            var reportDto = new LostReportDTO { PetName = "Fluffy" };
            var lostPetReport = new LostPetReport { Pet = pet, UserProfile = user };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, email)
            }, "mock"));

            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipal);
            _userRepositoryMock.Setup(x => x.GetUserbyEmail(email)).ReturnsAsync(user);
            _petsRepositoryMock.Setup(x => x.GetUserPets(user)).ReturnsAsync(new List<Pets> { pet });
            _mapperMock.Setup(x => x.Map<LostPetReport>(reportDto)).Returns(lostPetReport);
            _lostReportRepositoryMock.Setup(x => x.AddLostPetReportAsync(lostPetReport)).ReturnsAsync(1);

            // Act
            var result = await _lostReportService.CreateLostPetReportAsync(reportDto);

            // Assert
            Assert.True(result);
            _notificationRepositoryMock.Verify(x => x.AddNotificationAsync(It.IsAny<Notification>()), Times.Once);
        }

        [Fact]
        public async Task GetLostPetReportByCity_ShouldReturnNull_WhenCityIsNullOrEmpty()
        {
            // Act
            var result = await _lostReportService.GetLostPetReportByCity(null);

            // Assert
            Assert.Null(result);
            VerifyLogger(LogLevel.Error, "City is null or empty");
        }

        [Fact]
        public async Task GetLostPetReportByCity_ShouldReturnNull_WhenNoReportsFound()
        {
            // Arrange
            var city = "SampleCity";
            _lostReportRepositoryMock.Setup(x => x.GetLostPetReportByCityAsync(city)).ReturnsAsync((IEnumerable<LostPetReport>)null);

            // Act
            var result = await _lostReportService.GetLostPetReportByCity(city);

            // Assert
            Assert.Null(result);
            VerifyLogger(LogLevel.Error, "No reports found");
        }

        [Fact]
        public async Task GetLostPetReportByCity_ShouldReturnReports_WhenReportsFound()
        {
            // Arrange
            var city = "SampleCity";
            var lostPetReports = new List<LostPetReport>
            {
                new LostPetReport { Pet = new Pets { Name = "Fluffy" }, UserProfile = new UserProfile { Email = "test@example.com" } }
            };
            var lostPetResponseDTOs = new List<LostPetResponseDTO>
            {
                new LostPetResponseDTO { PetName = "Fluffy", }
            };

            _lostReportRepositoryMock.Setup(x => x.GetLostPetReportByCityAsync(city)).ReturnsAsync(lostPetReports);
            _mapperMock.Setup(x => x.Map<IEnumerable<LostPetResponseDTO>>(lostPetReports)).Returns(lostPetResponseDTOs);

            // Act
            var result = await _lostReportService.GetLostPetReportByCity(city);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Fluffy", result.First().PetName);
          
        }

        [Fact]
        public async Task FoundLostPet_ShouldReturnFalse_WhenIdIsInvalid()
        {
            // Act
            var result = await _lostReportService.FoundLostPet(-1);

            // Assert
            Assert.False(result);
            VerifyLogger(LogLevel.Error, "Invalid id");
        }

        [Fact]
        public async Task FoundLostPet_ShouldReturnTrue_WhenPetIsFound()
        {
            // Arrange
            var lostReportID = 1;
            _lostReportRepositoryMock.Setup(x => x.FoundLostPet(lostReportID)).ReturnsAsync(true);

            // Act
            var result = await _lostReportService.FoundLostPet(lostReportID);

            // Assert
            Assert.True(result);
        }

        private void VerifyLogger(LogLevel logLevel, string message)
        {
            _loggerMock.Verify(
                x => x.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
