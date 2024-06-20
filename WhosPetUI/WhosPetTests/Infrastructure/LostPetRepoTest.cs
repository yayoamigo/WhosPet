using Xunit;
using Moq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using WhosPetCore.Domain.Entities;
using WhosPetInfrastructure.Repos;
using WhosPetCore.DTO.Incoming.Pets;
using WhosPetAuth;
using System.Collections.Generic;

namespace WhosPetTests.Infrastructure.LostPetRepoTests
{
    [Collection("Sequential-Tests")]
    public class LostReportRepositoryTests : IAsyncLifetime
    {
        private readonly Mock<IDbConnection> _mockConnection;
        private readonly Mock<IDbCommand> _mockCommand;
        private readonly Mock<IDataReader> _mockDataReader;
        private readonly LostReportRepository _lostReportRepository;
        private readonly TestFixture _fixture;

        public LostReportRepositoryTests(TestFixture fixture)
        {
            _fixture = fixture;
            var options = new ConnectionStringOptions { ConnectionString = _fixture.ConnectionString };
            _lostReportRepository = new LostReportRepository(options);

            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockDataReader = new Mock<IDataReader>();

            _mockConnection.Setup(conn => conn.CreateCommand()).Returns(_mockCommand.Object);
            _mockConnection.Setup(conn => conn.Open()).Callback(() => { });

            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(1);
            _mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1);
            _mockCommand.Setup(cmd => cmd.ExecuteReader()).Returns(_mockDataReader.Object);
        }

        public Task InitializeAsync()
        {
            return _fixture.CleanUpData();
        }

        public Task DisposeAsync()
        {
            return _fixture.CleanUpData();
        }

        private async Task<int> SeedData()
        {
            using (var connection = new SqlConnection(_fixture.ConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    // Insert UserProfile
                    command.CommandText = @"
                    IF NOT EXISTS (SELECT 1 FROM UserProfiles WHERE Email = 'ownerd@example.com')
                    BEGIN
                        INSERT INTO UserProfiles (Email, Name, Surname, City, Address) 
                        VALUES ('ownerd@example.com', 'Johny', 'Doe', 'New York', '123 Main St');
                    END";
                    await command.ExecuteNonQueryAsync();

                    // Insert Pet data
                    command.CommandText = @"
                    INSERT INTO Pets (Name, Type, Breed, Color, City, Age, Description, Image, UserId) 
                    OUTPUT INSERTED.Id
                    VALUES ('Buddy', 'Dog', 'Labrador', 'Yellow', 'New York', 3, 'Friendly dog', 'image.jpg', 'ownerd@example.com')";

                    return (int)await command.ExecuteScalarAsync();
                }
            }
        }

        private LostPetReport CreateLostPetReport(int petId, string userEmail)
        {
            return new LostPetReport
            {
                Pet = new Pets { Id = petId, Name = "Buddy", City = "New York" },
                UserProfile = new UserProfile { Email = userEmail },
                PetName = "Buddy",
                Description = "Lost in Central Park",
                Date = DateTime.Now,
                City = "New York",
                longitude = -73.968285,
                latitude = 40.785091,
                Image = "buddy.jpg",
                IsFound = false,
                IsActive = true
            };
        }

        [Fact]
        public async Task AddLostPetReportAsync_ShouldReturnNewReportId()
        {
            // Arrange
            var petId = await SeedData();
            var userEmail = "ownerd@example.com";
            var lostPetReport = CreateLostPetReport(petId, userEmail);

            // Act
            var result = await _lostReportRepository.AddLostPetReportAsync(lostPetReport);

            // Assert
            Assert.True(result > 0);

            // Cleanup
            await _fixture.CleanUpData();
        }

        [Fact]
        public async Task GetLostPetReportByCityAsync_ShouldReturnReports()
        {
            // Arrange
            var petId = await SeedData();
            var userEmail = "ownerd@example.com";
            var lostPetReport = CreateLostPetReport(petId, userEmail);
            await _lostReportRepository.AddLostPetReportAsync(lostPetReport);

            _mockDataReader.SetupSequence(reader => reader.Read())
                .Returns(true)
                .Returns(false);
            _mockDataReader.Setup(reader => reader["Id"]).Returns(1);
            _mockDataReader.Setup(reader => reader["PetId"]).Returns(petId);
            _mockDataReader.Setup(reader => reader["UserId"]).Returns(userEmail);
            _mockDataReader.Setup(reader => reader["PetName"]).Returns("Buddy");
            _mockDataReader.Setup(reader => reader["Description"]).Returns("Lost in Central Park");
            _mockDataReader.Setup(reader => reader["Date"]).Returns(DateTime.Now);
            _mockDataReader.Setup(reader => reader["City"]).Returns("New York");
            _mockDataReader.Setup(reader => reader["Longitude"]).Returns(-73.968285);
            _mockDataReader.Setup(reader => reader["Latitude"]).Returns(40.785091);
            _mockDataReader.Setup(reader => reader["Image"]).Returns("buddy.jpg");
            _mockDataReader.Setup(reader => reader["IsFound"]).Returns(false);
            _mockDataReader.Setup(reader => reader["IsActive"]).Returns(true);

            // Act
            var result = await _lostReportRepository.GetLostPetReportByCityAsync("New York");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);

            // Cleanup
            await _fixture.CleanUpData();
        }

        [Fact]
        public async Task FoundLostPet_ShouldReturnTrue_WhenPetIsMarkedAsFound()
        {
            // Arrange
            var petId = await SeedData();
            var userEmail = "ownerd@example.com";
            var lostPetReport = CreateLostPetReport(petId, userEmail);
            var reportId = await _lostReportRepository.AddLostPetReportAsync(lostPetReport);

            // Act
            var result = await _lostReportRepository.FoundLostPet(reportId);

            // Assert
            Assert.True(result);

            // Cleanup
            await _fixture.CleanUpData();
        }
    }
}
