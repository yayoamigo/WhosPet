using Xunit;
using Moq;
using AlertaPatitasAPIUI.Controllers.PetsControllers;
using WhosPetCore.DTO.Incoming.Pets;
using WhosPetCore.ServiceContracts.PetContracts;
using WhosPetCore.DTO.Outgoing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AlertaPatitasAPIUI.Tests.Controllers.PetsControllers
{
    public class PetControllerTests
    {
        private readonly Mock<IAddPetService> _mockAddPetService;
        private readonly Mock<IGetPetService> _mockGetPetService;
        private readonly Mock<IUpdatePetService> _mockUpdatePetService;
        private readonly Mock<IDeletePetService> _mockDeletePetService;
        private readonly PetController _controller;

        public PetControllerTests()
        {
            _mockAddPetService = new Mock<IAddPetService>();
            _mockGetPetService = new Mock<IGetPetService>();
            _mockUpdatePetService = new Mock<IUpdatePetService>();
            _mockDeletePetService = new Mock<IDeletePetService>();

            _controller = new PetController(
                _mockAddPetService.Object,
                _mockGetPetService.Object,
                _mockUpdatePetService.Object,
                _mockDeletePetService.Object,
                null);
        }

        [Fact]
        public async Task AddPet_ReturnsOkResult_WhenPetIsAddedSuccessfully()
        {
            // Arrange
            var addPetDTO = new AddPetDTO { /* Set properties */ };
            var resultDto = new PetResponseDTO { Success = true, Details = new List<PetDetails> { new PetDetails { /* Set properties */ } } };
            _mockAddPetService.Setup(service => service.AddPet(addPetDTO)).ReturnsAsync(resultDto);

            // Act
            var result = await _controller.AddPet(addPetDTO);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resultDto.Details, okResult.Value);
        }

        [Fact]
        public async Task AddPet_ReturnsBadRequest_WhenPetAdditionFails()
        {
            // Arrange
            var addPetDTO = new AddPetDTO { /* Set properties */ };
            var resultDto = new PetResponseDTO { Success = false, Message = "Error adding pet" };
            _mockAddPetService.Setup(service => service.AddPet(addPetDTO)).ReturnsAsync(resultDto);

            // Act
            var result = await _controller.AddPet(addPetDTO);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(resultDto.Message, badRequestResult.Value);
        }

        [Fact]
        public async Task GetPetsByType_ReturnsOkResult_WithPets()
        {
            // Arrange
            var type = "Dog";
            var pets = new PetResponseDTO { Details = new List<PetDetails> { new PetDetails { /* Set properties */ } } };
            _mockGetPetService.Setup(service => service.GetPetByType(type)).ReturnsAsync(pets);

            // Act
            var result = await _controller.GetPetsByType(type);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pets, okResult.Value);
        }

        [Fact]
        public async Task GetPetsByType_ReturnsNotFound_WhenNoPetsFound()
        {
            // Arrange
            var type = "Dog";
            var pets = new PetResponseDTO { Details = new List<PetDetails>() };
            _mockGetPetService.Setup(service => service.GetPetByType(type)).ReturnsAsync(pets);

            // Act
            var result = await _controller.GetPetsByType(type);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeletePet_ReturnsOkResult_WhenPetIsDeleted()
        {
            // Arrange
            var petId = 1;
            _mockDeletePetService.Setup(service => service.DeletePet(petId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeletePet(petId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Pet deleted successfully", okResult.Value);
        }

        [Fact]
        public async Task DeletePet_ReturnsNotFound_WhenPetNotFound()
        {
            // Arrange
            var petId = 1;
            _mockDeletePetService.Setup(service => service.DeletePet(petId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeletePet(petId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Pet not found", notFoundResult.Value);
        } 
    }
}
