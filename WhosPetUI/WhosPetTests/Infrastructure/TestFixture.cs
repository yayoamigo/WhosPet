using System.Data.SqlClient;
using Xunit;

public class TestFixture : IAsyncLifetime
{
    public string ConnectionString { get; }

    public TestFixture()
    {
        ConnectionString = "Data Source=DESKTOP-ER2DF6Q;Initial Catalog=WhosPetTest;User ID=sa;Password=12345;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True";
    }

    public async Task InitializeAsync()
    {
        await CleanUpData();
    }

    public async Task DisposeAsync()
    {
        await CleanUpData();
    }

    public async Task CleanUpData()
    {
        using (var connection = new SqlConnection(ConnectionString))
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
