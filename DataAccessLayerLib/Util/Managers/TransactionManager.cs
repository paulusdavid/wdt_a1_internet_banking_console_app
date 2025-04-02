using Microsoft.Data.SqlClient;
using CommonLib.Data.Models;
using System;
using System.Collections.Generic;

namespace CommonLib.Util.Managers
{
    public class TransactionManager
    {
        private readonly string _connectionString;

        public TransactionManager(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task CreateTransactionTableIfNotExists()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check if the table exists
                var queryCheckTable = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                                        WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Transaction';";

                using (var commandCheckTable = new SqlCommand(queryCheckTable, connection))
                {
                    var tableExists = (int)await commandCheckTable.ExecuteScalarAsync() > 0;
                    if (tableExists)
                    {
                        Console.WriteLine("Transaction table already exists.");
                        return;
                    }
                }

                // Create the table if it does not exist
                var queryCreateTable = @"CREATE TABLE [dbo].[Transaction] (
                                            [TransactionID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                            [TransactionType] CHAR(1) NOT NULL CHECK (TransactionType IN ('D', 'W', 'T', 'S')),
                                            [AccountNumber] INT NOT NULL,
                                            [DestinationAccountNumber] INT NULL,
                                            [Amount] MONEY NOT NULL CHECK (Amount > 0),
                                            [Comment] NVARCHAR(30) NULL,
                                            [TransactionTimeUtc] DATETIME2 NOT NULL,
                                            CONSTRAINT [FK_Transaction_Account_AccountNumber] FOREIGN KEY ([AccountNumber]) REFERENCES [dbo].[Account] ([AccountNumber]),
                                            CONSTRAINT [FK_Transaction_Account_DestinationAccountNumber] FOREIGN KEY ([DestinationAccountNumber]) REFERENCES [dbo].[Account] ([AccountNumber]),
                                            CONSTRAINT [CH_Transaction_TransactionType] CHECK (TransactionType IN ('D', 'W', 'T', 'S'))
                                        );";

                using (var commandCreateTable = new SqlCommand(queryCreateTable, connection))
                {
                    await commandCreateTable.ExecuteNonQueryAsync();
                    Console.WriteLine("Transaction table created successfully.");
                }
            }
        }
        public void InsertTransaction(Transaction transaction)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "INSERT INTO [Transaction] (TransactionType, AccountNumber, DestinationAccountNumber, Amount, Comment, TransactionTimeUtc) VALUES (@TransactionType, @AccountNumber, @DestinationAccountNumber, @Amount, @Comment, @TransactionTimeUtc)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
                    command.Parameters.AddWithValue("@AccountNumber", transaction.AccountNumber);
                    command.Parameters.AddWithValue("@DestinationAccountNumber", (object)transaction.DestinationAccountNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Amount", transaction.Amount);
                    command.Parameters.AddWithValue("@Comment", string.IsNullOrEmpty(transaction.Comment) ? DBNull.Value : (object)transaction.Comment);
                    command.Parameters.AddWithValue("@TransactionTimeUtc", transaction.TransactionTimeUtc);
                    command.ExecuteNonQuery();
                }
            }
        }
        //Calculate transaction count based on account number
        public int GetTransactionCount(int accountNumber)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT COUNT(*) FROM [Transaction] WHERE AccountNumber = @AccountNumber";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    return (int)command.ExecuteScalar();
                }
            }
        }

        public List<Transaction> GetTransactionsByAccountNumber(int accountNumber)
        {
            var transactions = new List<Transaction>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT TransactionID, TransactionType, AccountNumber, DestinationAccountNumber, Amount, Comment, TransactionTimeUtc " +
                            "FROM [Transaction] WHERE AccountNumber = @AccountNumber OR DestinationAccountNumber = @AccountNumber";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var transaction = new Transaction
                            {
                                TransactionID = reader.GetInt32(0),
                                TransactionType = reader.GetString(1)[0],
                                AccountNumber = reader.GetInt32(2),
                                DestinationAccountNumber = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                                Amount = reader.GetDecimal(4),
                                Comment = reader.IsDBNull(5) ? null : reader.GetString(5),
                                TransactionTimeUtc = reader.GetDateTime(6)
                            };
                            transactions.Add(transaction);
                        }
                    }
                }
            }

            return transactions;
        }
    }
}
