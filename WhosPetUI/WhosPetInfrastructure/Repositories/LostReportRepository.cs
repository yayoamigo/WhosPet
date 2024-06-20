using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using WhosPetAuth;
using WhosPetCore.Domain.Entities;
using WhosPetCore.Domain.RepoContracts;
using WhosPetCore.DTO.Incoming.Pets;


namespace WhosPetInfrastructure.Repos
{
    public class LostReportRepository : ILostReportRepository
    {
        private readonly string _connectionString;

        public LostReportRepository(ConnectionStringOptions options)
        {
            _connectionString = options.ConnectionString;
        }

        public async Task<int> AddLostPetReportAsync(LostPetReport lostPetReport)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(
                    "INSERT INTO LostPetReports (PetId, UserId, PetName, Description, Date, City, Longitude, Latitude, Image, IsFound, IsActive) " +
                    "OUTPUT INSERTED.ID " +
                    "VALUES (@PetId, @UserId, @PetName, @Description, @Date, @City, @Longitude, @Latitude, @Image, @IsFound, @IsActive)", connection);
                command.Parameters.AddWithValue("@PetId", lostPetReport.Pet.Id);
                command.Parameters.AddWithValue("@UserId", lostPetReport.UserProfile.Email);
                command.Parameters.AddWithValue("@PetName", lostPetReport.PetName);
                command.Parameters.AddWithValue("@Description", lostPetReport.Description);
                command.Parameters.AddWithValue("@Date", lostPetReport.Date);
                command.Parameters.AddWithValue("@City", lostPetReport.City);
                command.Parameters.AddWithValue("@Longitude", lostPetReport.longitude);
                command.Parameters.AddWithValue("@Latitude", lostPetReport.latitude);
                command.Parameters.AddWithValue("@Image", lostPetReport.Image);
                command.Parameters.AddWithValue("@IsFound", lostPetReport.IsFound);
                command.Parameters.AddWithValue("@IsActive", lostPetReport.IsActive);

                await connection.OpenAsync();
                return (int)await command.ExecuteScalarAsync();
            }
        }

        

        public async Task<IEnumerable<LostPetReport>> GetLostPetReportByCityAsync(string City)
        {
            var reports = new List<LostPetReport>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM LostPetReports WHERE City = @City", connection);
                command.Parameters.AddWithValue("@City", City);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        reports.Add(MapReaderToLostPetReport(reader));
                    }
                }
            }
            return reports;
        }

        public async Task<bool> FoundLostPet(int lostReportID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("UPDATE LostPetReports SET IsFound = 1 WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", lostReportID);

                await connection.OpenAsync();
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
        }

        private LostPetReport MapReaderToLostPetReport(SqlDataReader reader)
        {
            return new LostPetReport
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                PetId = reader.GetInt32(reader.GetOrdinal("PetId")),
                UserId = reader.GetString(reader.GetOrdinal("UserId")),
                PetName = reader.GetString(reader.GetOrdinal("PetName")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                City = reader.GetString(reader.GetOrdinal("City")),
                longitude = reader.GetDouble(reader.GetOrdinal("Longitude")),
                latitude = reader.GetDouble(reader.GetOrdinal("Latitude")),
                Image = reader.GetString(reader.GetOrdinal("Image")),
                IsFound = reader.GetBoolean(reader.GetOrdinal("IsFound")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }
}
