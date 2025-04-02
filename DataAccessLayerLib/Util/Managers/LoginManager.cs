using Microsoft.Data.SqlClient;
using CommonLib.Data.Models;
using Azure;

namespace CommonLib.Util.Managers
{
    //Manages database operations related to login credentials.
    public class LoginManager
    {
        private readonly string _connectionString;

        // Initializes a new instance of the LoginManager with a specific database connection string.
        public LoginManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Asynchronously inserts a new login record into the database.
        public async Task InsertLogin(Login login)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "INSERT INTO Login (LoginID, CustomerID, PasswordHash) VALUES (@LoginID, @CustomerID, @PasswordHash)",
                    connection))
                {
                    command.Parameters.AddWithValue("@LoginID", login.LoginID);
                    command.Parameters.AddWithValue("@CustomerID", login.CustomerID);
                    command.Parameters.AddWithValue("@PasswordHash", login.PasswordHash);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task CreateLoginTableIfNotExists()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check if the table exists
                var queryCheckTable = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                                        WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Login';";

                using (var commandCheckTable = new SqlCommand(queryCheckTable, connection))
                {
                    var tableExists = (int)await commandCheckTable.ExecuteScalarAsync() > 0;
                    if (tableExists)
                    {
                        Console.WriteLine("Login table already exists.");
                        return;
                    }
                }

                // Create the table if it does not exist
                var queryCreateTable = @"CREATE TABLE [dbo].[Login] (
                                            [LoginID] CHAR(8) NOT NULL PRIMARY KEY,
                                            [CustomerID] INT NOT NULL,
                                            [PasswordHash] CHAR(94) NOT NULL,
                                            CONSTRAINT [FK_Login_Customer] FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer] ([CustomerID]),
                                            CONSTRAINT [CH_Login_LoginID] CHECK (LEN([LoginID]) = 8),
                                            CONSTRAINT [CH_Login_PasswordHash] CHECK (LEN([PasswordHash]) = 94)
                                        );";

                using (var commandCreateTable = new SqlCommand(queryCreateTable, connection))
                {
                    await commandCreateTable.ExecuteNonQueryAsync();
                    Console.WriteLine("Login table created successfully.");
                }
            }
        }
        // Asynchronously retrieves a login record by login ID.
        public async Task<Login> GetLogin(string loginId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT LoginID, CustomerID, PasswordHash FROM Login WHERE LoginID = @LoginID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LoginID", loginId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Login
                            {
                                LoginID = reader.GetString(0),
                                CustomerID = reader.GetInt32(1),
                                PasswordHash = reader.GetString(2)
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        // Asynchronously retrieves a CustomerID associated with a given LoginID.
        public async Task<int> GetCustomerID(int loginID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT CustomerID FROM Login WHERE LoginID = @LoginID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LoginID", loginID);
                    var result = await command.ExecuteScalarAsync(); 

                    if (result != null)
                    {
                        return Convert.ToInt32(result); 
                    }
                    else
                    {
                        return 0; 
                    }
                }
            }
        }
    }
}
