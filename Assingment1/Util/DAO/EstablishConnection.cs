using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Assignment1.Util.DAO
{
    public class EstablishConnection
    {
        private readonly IConfiguration _configuration;

        public EstablishConnection()
        {
            // Build the configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public SqlConnection GetConnection()
        {
            // Fetch the connection string from the configuration
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Establish and return the SQL connection
            return new SqlConnection(connectionString);
        }
    }
}
