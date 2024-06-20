using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WhosPetAuth;
using WhosPetCore.Domain.Entities;
using WhosPetCore.Domain.RepoContracts;

namespace WhosPetInfrastructure.Repos
{
    public class PetsRepository : IPetsRepository
    {
        private readonly string _connectionString;

        public PetsRepository(ConnectionStringOptions options)
        {
            _connectionString = options.ConnectionString;
        }

        public async Task<int> AddPet(Pets pet)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    "INSERT INTO Pets (Name, Type, Breed, Color, City, Age, Description, Image, UserId) " +
                    "OUTPUT INSERTED.Id " +
                    "VALUES (@Name, @Type, @Breed, @Color, @City, @Age, @Description, @Image, @UserId)", connection);
                command.Parameters.AddWithValue("@Name", pet.Name);
                command.Parameters.AddWithValue("@Type", pet.Type);
                command.Parameters.AddWithValue("@Breed", pet.Breed);
                command.Parameters.AddWithValue("@Color", pet.Color);
                command.Parameters.AddWithValue("@City", pet.City);
                command.Parameters.AddWithValue("@Age", pet.Age);
                command.Parameters.AddWithValue("@Description", pet.Description);
                command.Parameters.AddWithValue("@Image", pet.Image);
                command.Parameters.AddWithValue("@UserId", pet.Owner.Email);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                return (int)result;
            }
        }


        public async Task<List<Pets>> GetPetByType(string type)
        {
            var pets = new List<Pets>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM Pets WHERE Type = @Type", connection);
                command.Parameters.AddWithValue("@Type", type.ToString());

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        pets.Add(MapReaderToPet(reader));
                    }
                }
            }
            return pets;
        }

        public async Task<List<Pets>> GetPetsByCity(string city)
        {
            var pets = new List<Pets>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM Pets WHERE City = @City", connection);
                command.Parameters.AddWithValue("@City", city);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        pets.Add(MapReaderToPet(reader));
                    }
                }
            }
            return pets;
        }

        public async Task<List<Pets>> GetPetsByShelter(string name)
        {
            var pets = new List<Pets>();
            using (var connection = new SqlConnection(_connectionString))
            {
               
                var command = new SqlCommand(@"
            SELECT p.* 
            FROM Pets p
            INNER JOIN UserProfiles u ON p.UserId = u.Email
            WHERE u.Name = @Name", connection);

                command.Parameters.AddWithValue("@Name", name);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        pets.Add(MapReaderToPet(reader));
                    }
                }
            }
            return pets;
        }


        public async Task<List<Pets>> GetUserPets(UserProfile user)
        {
            var pets = new List<Pets>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM Pets WHERE UserId = @UserId", connection);
                command.Parameters.AddWithValue("@UserId", user.Email);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        pets.Add(MapReaderToPet(reader));
                    }
                }
            }
            return pets;
        }

        public async Task<bool> UpdatePet(Pets pet)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    "UPDATE Pets SET Name = @Name, Type = @Type, Breed = @Breed, Color = @Color, Age = @Age, " +
                    "Description = @Description, Image = @Image WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", pet.Id); 
                command.Parameters.AddWithValue("@Name", pet.Name);
                command.Parameters.AddWithValue("@Type", pet.Type);
                command.Parameters.AddWithValue("@Breed", pet.Breed);
                command.Parameters.AddWithValue("@Color", pet.Color);               
                command.Parameters.AddWithValue("@Age", pet.Age);
                command.Parameters.AddWithValue("@Description", pet.Description);
                command.Parameters.AddWithValue("@Image", pet.Image);

                await connection.OpenAsync();
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        public async Task<bool> DeletePet(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("DELETE FROM Pets WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                await connection.OpenAsync();
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        private Pets MapReaderToPet(SqlDataReader reader)
        {
            return new Pets
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Type = reader.GetString(reader.GetOrdinal("Type")),
                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                Color = reader.GetString(reader.GetOrdinal("Color")),
                City = reader.GetString(reader.GetOrdinal("City")),
                Age = reader.GetInt32(reader.GetOrdinal("Age")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Image = reader.GetString(reader.GetOrdinal("Image")),
                UserId = reader.GetString(reader.GetOrdinal("UserId"))
               
            };
        }
    }
}
