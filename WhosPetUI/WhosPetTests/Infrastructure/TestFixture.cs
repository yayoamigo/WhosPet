using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Data.SqlClient;

namespace WhosPetTests.Infrastructure
{
    public class TestFixture : IAsyncLifetime
    {
        private readonly string _connectionString = "Data Source=DESKTOP-ER2DF6Q;Initial Catalog=WhosPetTest;User ID=sa;Password=12345;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True";
        public TestFixture()
        {
           
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
                    DELETE FROM LostPetReports;
                    DELETE FROM Notifications;
                    DELETE FROM Pets;
                    DELETE FROM UserProfiles;";
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }

}
