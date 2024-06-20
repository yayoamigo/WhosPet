using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using WhosPetAuth;
using WhosPetCore.Domain.Entities;
using WhosPetInfrastructure.Repos;
using Xunit;
using Moq;

namespace WhosPetTests.Infrastructure.LostPet
{
    public class PetsRepositoryTests : IAsyncLifetime
    {
        private readonly Mock<IDbConnection> _mockConnection;
        private readonly Mock<IDbCommand> _mockCommand;
        private readonly Mock<IDataReader> _mockDataReader;
        private readonly PetsRepository _petsRepository;
        private readonly string _connectionString = "Data Source=DESKTOP-ER2DF6Q;Initial Catalog=WhosPetTest;User ID=sa;Password=12345;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True";

        public PetsRepositoryTests()
        {
            var options = new ConnectionStringOptions { ConnectionString = _connectionString };
            _petsRepository = new PetsRepository(options);

            _mockConnection = new Mock<IDbConnection>();
            _mockCommand = new Mock<IDbCommand>();
            _mockDataReader = new Mock<IDataReader>();

            _mockConnection.Setup(conn => conn.CreateCommand()).Returns(_mockCommand.Object);
            _mockConnection.Setup(conn => conn.Open()).Callback(() => {  });

            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(1);
            _mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1);
            _mockCommand.Setup(cmd => cmd.ExecuteReader()).Returns(_mockDataReader.Object);
        }

        public async Task InitializeAsync()
        {
            await CleanUpData();
        }

        public async Task DisposeAsync()
        {
            await CleanUpData();
        }

        private async Task CleanUpData()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        DELETE FROM Pets;
                        DELETE FROM UserProfiles;";
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<int> SeedData()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    // Insert UserProfile
                    command.CommandText = @"
                    IF NOT EXISTS (SELECT 1 FROM UserProfiles WHERE Email = 'owner@example.com')
                    BEGIN
                        INSERT INTO UserProfiles (Email, Name, Surname, City, Address) 
                        VALUES ('owner@example.com', 'John', 'Doe', 'New York', '123 Main St');
                    END";
                    await command.ExecuteNonQueryAsync();

                    // Insert Pet data
                    command.CommandText = @"
                    INSERT INTO Pets (Name, Type, Breed, Color, City, Age, Description, Image, UserId) 
                    OUTPUT INSERTED.Id
                    VALUES ('Buddy', 'Dog', 'Labrador', 'Yellow', 'New York', 3, 'Friendly dog', 'image.jpg', 'owner@example.com')";

                    return (int)await command.ExecuteScalarAsync();
                }
            }
        }

        private async Task SeedDataForAdding()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;

                    // Insert UserProfile
                    command.CommandText = @"
                    IF NOT EXISTS (SELECT 1 FROM UserProfiles WHERE Email = 'owner2@example.com')
                    BEGIN
                        INSERT INTO UserProfiles (Email, Name, Surname, City, Address) 
                        VALUES ('owner2@example.com', 'John', 'Doe', 'New York', '123 Main St');
                    END";
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        [Fact]
        public async Task AddPet_ShouldReturnNewPetId()
        {
            // Arrange
            await SeedDataForAdding();

            var user = new UserProfile
            {
                Email = "owner2@example.com"
            };

            var pet = new Pets
            {
                Name = "Buddy2",
                Type = "Dog",
                Breed = "Labrador",
                Color = "Yellow",
                City = "New York",
                Age = 3,
                Description = "Friendly dog",
                Image = "image.jpg",
                Owner = user
            };

            // Act
            var result = await _petsRepository.AddPet(pet);

            // Assert
            Assert.True(result > 0);
        }

        [Fact]
        public async Task GetPetsByCity_ShouldReturnPets()
        {
            // Arrange
            var id = await SeedData();

            _mockDataReader.SetupSequence(reader => reader.Read())
                .Returns(true)
                .Returns(false);
            _mockDataReader.Setup(reader => reader["Id"]).Returns(id);
            _mockDataReader.Setup(reader => reader["Name"]).Returns("Buddy");
            _mockDataReader.Setup(reader => reader["Type"]).Returns("Dog");
            _mockDataReader.Setup(reader => reader["Breed"]).Returns("Labrador");
            _mockDataReader.Setup(reader => reader["Color"]).Returns("Yellow");
            _mockDataReader.Setup(reader => reader["City"]).Returns("New York");
            _mockDataReader.Setup(reader => reader["Age"]).Returns(3);
            _mockDataReader.Setup(reader => reader["Description"]).Returns("Friendly dog");
            _mockDataReader.Setup(reader => reader["Image"]).Returns("image.jpg");
            _mockDataReader.Setup(reader => reader["UserId"]).Returns("owner@example.com");

            // Act
            var result = await _petsRepository.GetPetsByCity("New York");

            // Assert
            Assert.True(result.Count > 0);

        }

        [Fact]
        public async Task DeletePet_ShouldReturnTrue()
        {
            // Arrange
            var id = await SeedData();

            // Act
            var result = await _petsRepository.DeletePet(id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdatePet_ShouldReturnTrue()
        {
            // Arrange
            var id = await SeedData();

            var pet = new Pets
            {
                Id = id,
                Name = "Buddy",
                Type = "Dog",
                Breed = "Labrador",
                Color = "Yellow",
                City = "New York",
                Age = 3,
                Description = "Friendly dog",
                Image = "image.jpg",
                UserId = "owner@example.com"
            };

            // Act
            var result = await _petsRepository.UpdatePet(pet);

            // Assert
            Assert.True(result);
        }
    }
}
