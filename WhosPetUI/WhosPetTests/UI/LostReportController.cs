using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WhosPetCore.ServiceContracts.ReportContracts;
using WhosPetCore.DTO.Incoming.Pets;
using WhosPetCore.DTO.Outgoing;
using WhosPetUI.Controllers.ReportingControllers;
using System.Collections.Generic;

namespace WhosPetTests.Controllers.ReportingControllers
{
    public class LostPetReportControllerTests
    {
        private readonly Mock<IAddLostReport> _mockAddLostReportService;
        private readonly Mock<IGetLostReports> _mockGetLostReportsService;
        private readonly Mock<IUpdatePetReport> _mockUpdatePetReportService;
        private readonly Mock<ILogger<LostPetReportController>> _mockLogger;
        private readonly LostPetReportController _controller;

        public LostPetReportControllerTests()
        {
            _mockAddLostReportService = new Mock<IAddLostReport>();
            _mockGetLostReportsService = new Mock<IGetLostReports>();
            _mockUpdatePetReportService = new Mock<IUpdatePetReport>();
            _mockLogger = new Mock<ILogger<LostPetReportController>>();

            _controller = new LostPetReportController(
                _mockAddLostReportService.Object,
                _mockGetLostReportsService.Object,
                _mockUpdatePetReportService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CreateLostPetReport_ReturnsOk_WhenReportIsCreatedSuccessfully()
        {
            // Arrange
            var reportDto = new LostReportDTO();
            _mockAddLostReportService.Setup(service => service.CreateLostPetReportAsync(reportDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.CreateLostPetReport(reportDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Lost pet report created successfully", okResult.Value);
        }

        [Fact]
        public async Task CreateLostPetReport_ReturnsStatusCode500_WhenReportCreationFails()
        {
            // Arrange
            var reportDto = new LostReportDTO();
            _mockAddLostReportService.Setup(service => service.CreateLostPetReportAsync(reportDto)).ReturnsAsync(false);

            // Act
            var result = await _controller.CreateLostPetReport(reportDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Failed to create lost pet report", statusCodeResult.Value);
        }

        [Fact]
        public async Task GetLostPetReportsByCity_ReturnsBadRequest_WhenCityIsNullOrEmpty()
        {
            // Act
            var result = await _controller.GetLostPetReportsByCity(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("City cannot be null or empty", badRequestResult.Value);
        }

        [Fact]
        public async Task GetLostPetReportsByCity_ReturnsOk_WhenReportsAreFound()
        {
            // Arrange
            var city = "SampleCity";
            var reports = new List<LostPetResponseDTO>
            {
                new LostPetResponseDTO { PetName = "Fluffy" }
            };

            _mockGetLostReportsService.Setup(service => service.GetLostPetReportByCity(city)).ReturnsAsync(reports);

            // Act
            var result = await _controller.GetLostPetReportsByCity(city);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(reports, okResult.Value);
        }

        [Fact]
        public async Task GetLostPetReportsByCity_ReturnsNotFound_WhenNoReportsAreFound()
        {
            // Arrange
            var city = "SampleCity";
            _mockGetLostReportsService.Setup(service => service.GetLostPetReportByCity(city)).ReturnsAsync((IEnumerable<LostPetResponseDTO>)null);

            // Act
            var result = await _controller.GetLostPetReportsByCity(city);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"No lost pet reports found for: {city}", notFoundResult.Value);
        }

        [Fact]
        public async Task FoundLostPet_ReturnsBadRequest_WhenIdIsZero()
        {
            // Act
            var result = await _controller.FoundLostPet(0);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Id cannot be null or empty", badRequestResult.Value);
        }

        [Fact]
        public async Task FoundLostPet_ReturnsOk_WhenPetIsFoundSuccessfully()
        {
            // Arrange
            var id = 1;
            _mockUpdatePetReportService.Setup(service => service.FoundLostPet(id)).ReturnsAsync(true);

            // Act
            var result = await _controller.FoundLostPet(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal($"Lost pet report with id: {id} marked as found successfully", okResult.Value);
        }

        [Fact]
        public async Task FoundLostPet_ReturnsStatusCode500_WhenMarkingAsFoundFails()
        {
            // Arrange
            var id = 1;
            _mockUpdatePetReportService.Setup(service => service.FoundLostPet(id)).ReturnsAsync(false);

            // Act
            var result = await _controller.FoundLostPet(id);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal($"Failed to mark lost pet report with id: {id} as found", statusCodeResult.Value);
        }
    }
}
