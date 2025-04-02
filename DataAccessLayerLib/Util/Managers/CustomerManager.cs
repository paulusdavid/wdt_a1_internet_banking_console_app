using CommonLib.Data.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace CommonLib.Util.Managers
{
    // Manages database operations related to customers.
    public class CustomerManager
    {
        private readonly string _connectionString;

        // Initializes a new instance of the CustomerManager with a specific database connection string.
        public CustomerManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Asynchronously inserts a customer into the database.
        public async Task InsertCustomer(Customer customer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                //Open asynchronous connection
                await connection.OpenAsync();
                var query = @"INSERT INTO Customer (CustomerID, Name, Address, City, PostCode) 
                              VALUES (@CustomerID, @Name, @Address, @City, @PostCode)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
                    command.Parameters.AddWithValue("@Name", customer.Name);
                    command.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@City", customer.City ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PostCode", customer.PostCode ?? (object)DBNull.Value);

                    //execute queries in asynchronous manner
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Checks if a customer exists in the database by CustomerID.
        public bool Exists(int customerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT COUNT(*) FROM Customer WHERE CustomerID = @CustomerID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    var count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }
        public async Task CreateCustomerTableIfNotExists()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Customer')
                          CREATE TABLE Customer
                          (
                              CustomerID int not null,
                              Name nvarchar(50) not null,
                              Address nvarchar(50) null,
                              City nvarchar(40) null,
                              PostCode nvarchar(4) null,
                              constraint PK_Customer primary key (CustomerID)
                          )";

                    using (var command = new SqlCommand(query, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                Console.WriteLine("Customer table created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Customer table: {ex.Message}");
                throw;
            }
        }
        // Asynchronously retrieves a customer by CustomerID.
        public async Task<Customer> GetCustomerById(int customerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT CustomerID, Name, Address, City, PostCode FROM Customer WHERE CustomerID = @CustomerID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            return new Customer
                            {
                                CustomerID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Address = reader.IsDBNull(2) ? null : reader.GetString(2),
                                City = reader.IsDBNull(3) ? null : reader.GetString(3),
                                PostCode = reader.IsDBNull(4) ? null : reader.GetString(4)
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}