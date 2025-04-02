using DataAccessLayerLib.Util.Managers;
using CommonLib.Data.Models;
using System;
using CommonLib.Util.Managers;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using CommonLib.Util.Managers;
namespace DataAccessLayerLib.Util.Managers
{
    public class AccountManager
    {
        private readonly string _connectionString;
        private readonly TransactionManager _transactionManager;
        private const decimal ATMWithdrawFee = 0.05m; // A$0.05 for ATM withdrawal
        private const decimal AccountTransferFee = 0.10m; // A$0.10 for account transfer
        public AccountManager(string connectionString, TransactionManager transactionManager)
        {
            _connectionString = connectionString;
            _transactionManager = transactionManager;
        }

        public async Task CreateAccountTableIfNotExists()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check if the table exists
                var queryCheckTable = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                                        WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Account';";

                using (var commandCheckTable = new SqlCommand(queryCheckTable, connection))
                {
                    var tableExists = (int)await commandCheckTable.ExecuteScalarAsync() > 0;
                    if (tableExists)
                    {
                        Console.WriteLine("Account table already exists.");
                        return;
                    }
                }

                // Create the table if it does not exist
                var queryCreateTable = @"CREATE TABLE [dbo].[Account] (
                                            [AccountNumber] INT NOT NULL PRIMARY KEY,
                                            [AccountType] CHAR(1) NOT NULL CHECK (AccountType IN ('C', 'S')), -- 'C' for Checking, 'S' for Savings
                                            [CustomerID] INT NOT NULL,
                                            [Balance] MONEY NOT NULL,
                                            [TransactionCount] INT DEFAULT 0 NOT NULL, -- Adding TransactionCount with default 0
                                            CONSTRAINT [FK_Account_Customer] FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer] ([CustomerID]),
                                            CONSTRAINT [CH_Account_MinBalance] CHECK (
                                                (AccountType = 'S' AND Balance >= 0) OR 
                                                (AccountType = 'C' AND Balance >= 300)
                                            ), -- Minimum balance check based on account type
                                            CONSTRAINT [CH_Account_AccountType] CHECK (AccountType IN ('C', 'S'))
                                            CONSTRAINT UQ_CustomerID_AccountType UNIQUE (CustomerID, AccountType) -- Unique constraint to prevent multiple accounts of the same type per customer
                                        );";

                using (var commandCreateTable = new SqlCommand(queryCreateTable, connection))
                {
                    await commandCreateTable.ExecuteNonQueryAsync();
                    Console.WriteLine("Account table created successfully.");
                }
            }
        }

        public async Task InsertAccount(Account account)
        {
            // Check minimum balance requirement for opening the account
            if ((account.AccountType == 'S' && account.Balance < 50) ||
                (account.AccountType == 'C' && account.Balance < 500))
            {
                throw new ArgumentException($"Minimum balance requirement not met for {account.AccountType} account.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Verify that the CustomerID exists
                var checkCustomerQuery = "SELECT COUNT(*) FROM Customer WHERE CustomerID = @CustomerID";
                using (var checkCustomerCommand = new SqlCommand(checkCustomerQuery, connection))
                {
                    checkCustomerCommand.Parameters.AddWithValue("@CustomerID", account.CustomerID);
                    var customerExists = (int)await checkCustomerCommand.ExecuteScalarAsync() > 0;
                    if (!customerExists)
                    {
                        throw new InvalidOperationException($"Customer with ID {account.CustomerID} does not exist.");
                    }
                }

                // Verify that the customer does not already have an account of the same type
                var checkAccountTypeQuery = @"SELECT COUNT(*) FROM Account WHERE CustomerID = @CustomerID AND AccountType = @AccountType";
                using (var checkAccountTypeCommand = new SqlCommand(checkAccountTypeQuery, connection))
                {
                    checkAccountTypeCommand.Parameters.AddWithValue("@CustomerID", account.CustomerID);
                    checkAccountTypeCommand.Parameters.AddWithValue("@AccountType", account.AccountType);
                    var accountTypeExists = (int)await checkAccountTypeCommand.ExecuteScalarAsync() > 0;
                    if (accountTypeExists)
                    {
                        throw new InvalidOperationException($"Customer already has a {account.AccountType} account.");
                    }
                }

                // Insert new account if validations pass
                var query = @"INSERT INTO Account (AccountNumber, AccountType, CustomerID, Balance) 
                      VALUES (@AccountNumber, @AccountType, @CustomerID, @Balance)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AccountNumber", account.AccountNumber);
                    command.Parameters.AddWithValue("@AccountType", account.AccountType);
                    command.Parameters.AddWithValue("@CustomerID", account.CustomerID);
                    command.Parameters.AddWithValue("@Balance", account.Balance);

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Account> GetAccountsByCustomerId(int customerId)
        {
            var accounts = new List<Account>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT AccountNumber, AccountType, CustomerID, Balance FROM Account WHERE CustomerID = @CustomerID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var account = new Account
                            {
                                AccountNumber = reader.GetInt32(0),
                                AccountType = reader.GetString(1)[0],
                                CustomerID = reader.GetInt32(2),
                                Balance = reader.GetDecimal(3)
                            };
                            accounts.Add(account);
                        }
                    }
                }
            }
            return accounts;
        }

        public void Deposit(int accountNumber, decimal amount, string comment)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Deposit amount must be greater than zero.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Check the number of transactions for this account
                var transactionCountQuery = "SELECT TransactionCount FROM Account WHERE AccountNumber = @AccountNumber";
                using (var transactionCountCommand = new SqlCommand(transactionCountQuery, connection))
                {
                    transactionCountCommand.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    var transactionCount = (int)transactionCountCommand.ExecuteScalar();

                    // Determine if a fee needs to be applied
                    decimal transactionFee = 0;
                    if (transactionCount >= 2)
                    {
                        transactionFee = ATMWithdrawFee; // Apply fee only after 2 transactions
                    }

                    // Update account balance and transaction count
                    var updateAccountQuery = @"UPDATE Account 
                                       SET Balance = Balance + @Amount, 
                                           TransactionCount = TransactionCount + 1 
                                       WHERE AccountNumber = @AccountNumber";
                    using (var updateAccountCommand = new SqlCommand(updateAccountQuery, connection))
                    {
                        updateAccountCommand.Parameters.AddWithValue("@Amount", amount);
                        updateAccountCommand.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        updateAccountCommand.ExecuteNonQuery();
                    }

                    // Insert transaction record
                    var transaction = new Transaction
                    {
                        TransactionType = 'D',
                        AccountNumber = accountNumber,
                        Amount = amount,
                        Comment = comment,
                        TransactionTimeUtc = DateTime.UtcNow
                    };
                    _transactionManager.InsertTransaction(transaction);

                }
            }
        }


        public void Withdraw(int accountNumber, decimal amount, string comment)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Withdrawal amount must be greater than zero.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Retrieve current account balance, account type, and transaction count
                decimal currentBalance;
                char accountType;
                int transactionCount;
                var getAccountDetailsQuery = "SELECT Balance, AccountType, TransactionCount FROM Account WHERE AccountNumber = @AccountNumber";
                using (var command = new SqlCommand(getAccountDetailsQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new InvalidOperationException($"Account with number {accountNumber} not found.");
                        }
                        currentBalance = reader.GetDecimal(0);
                        accountType = reader.GetString(1)[0];
                        transactionCount = reader.GetInt32(2);
                    }
                }

                // Determine minimum balance allowed based on account type
                var minBalanceAllowed = (accountType == 'S' ? 0 : 300);

                // Check transaction limits for applying fees
                bool isFeeApplicable = transactionCount >= 2;

                // Check if the withdrawal amount exceeds the current balance or falls below the minimum allowed
                if (amount > currentBalance || currentBalance - amount < minBalanceAllowed)
                {
                    throw new InvalidOperationException("Insufficient funds or below minimum balance allowed.");
                }

                // Perform the withdrawal
                var query = "UPDATE Account SET Balance = Balance - @Amount, TransactionCount = TransactionCount + 1 WHERE AccountNumber = @AccountNumber";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("Failed to withdraw amount.");
                    }
                }

                // Insert withdrawal transaction
                var transaction = new Transaction
                {
                    TransactionType = 'W',
                    AccountNumber = accountNumber,
                    Amount = amount,
                    Comment = comment,
                    TransactionTimeUtc = DateTime.UtcNow
                };
                _transactionManager.InsertTransaction(transaction);

                // Apply ATM withdrawal fee if applicable
                if (isFeeApplicable)
                {
                    ApplyATMWithdrawFee(accountNumber);
                }
            }
        }


        public void Transfer(int accountNumber, int destinationAccountNumber, decimal amount, string comment)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Transfer amount must be greater than zero.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        // Check source account balance
                        decimal sourceBalance;
                        char sourceAccountType;
                        var checkBalanceQuery = "SELECT Balance, AccountType FROM Account WHERE AccountNumber = @SourceAccountNumber";

                        using (var checkBalanceCommand = new SqlCommand(checkBalanceQuery, connection, transaction))
                        {
                            checkBalanceCommand.Parameters.AddWithValue("@SourceAccountNumber", accountNumber);

                            using (var reader = checkBalanceCommand.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    throw new InvalidOperationException($"Source account with number {accountNumber} not found.");
                                }

                                sourceBalance = reader.GetDecimal(0);
                                sourceAccountType = reader.GetString(1)[0];
                            }
                        }

                        // Check if source account has sufficient balance
                        var minBalanceAllowed = (sourceAccountType == 'S' ? 0 : 300);

                        if (amount > sourceBalance || sourceBalance - amount < minBalanceAllowed)
                        {
                            throw new InvalidOperationException("Insufficient funds or below minimum balance allowed in source account.");
                        }

                        // Withdraw from source account including transfer fee
                        var totalAmountToWithdraw = amount + AccountTransferFee;
                        var withdrawQuery = "UPDATE Account SET Balance = Balance - @Amount WHERE AccountNumber = @SourceAccountNumber";

                        using (var withdrawCommand = new SqlCommand(withdrawQuery, connection, transaction))
                        {
                            withdrawCommand.Parameters.AddWithValue("@Amount", totalAmountToWithdraw);
                            withdrawCommand.Parameters.AddWithValue("@SourceAccountNumber", accountNumber);

                            int withdrawRowsAffected = withdrawCommand.ExecuteNonQuery();
                            if (withdrawRowsAffected == 0)
                            {
                                throw new InvalidOperationException("Failed to withdraw amount from the source account.");
                            }
                        }

                        // Deposit into destination account
                        var depositQuery = "UPDATE Account SET Balance = Balance + @Amount WHERE AccountNumber = @DestinationAccountNumber";

                        using (var depositCommand = new SqlCommand(depositQuery, connection, transaction))
                        {
                            depositCommand.Parameters.AddWithValue("@Amount", amount);
                            depositCommand.Parameters.AddWithValue("@DestinationAccountNumber", destinationAccountNumber);

                            int depositRowsAffected = depositCommand.ExecuteNonQuery();
                            if (depositRowsAffected == 0)
                            {
                                throw new InvalidOperationException("Failed to deposit amount into the destination account.");
                            }
                        }

                        // Insert transfer transaction
                        var transactionType = 'T'; // Transaction type for transfer
                        InsertTransaction(connection, transaction, transactionType, accountNumber, destinationAccountNumber, amount, comment);
                        transaction.Commit();

                        // Apply ATM transfer fee for the source account
                        ApplyATMTransferFee(accountNumber);
                        Console.WriteLine("Transfer completed successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred during transfer: {ex.Message}");
                        transaction.Rollback();
                        throw; // Rethrow the exception to halt further processing
                    }
                }
            }
        }
        private void ApplyATMTransferFee(int accountNumber)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Apply ATM transfer fee
                    var query = "UPDATE Account SET Balance = Balance - @Fee WHERE AccountNumber = @AccountNumber";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Fee", AccountTransferFee);
                        command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            throw new InvalidOperationException("Failed to apply transfer fee.");
                        }
                    }

                    // Insert service charge transaction
                    InsertServiceChargeTransaction(accountNumber, AccountTransferFee, "ATM transfer fee");

                    Console.WriteLine("Transfer fee applied successfully.");
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL Exception occurred in ApplyATMTransferFee: {sqlEx.Message}");
                throw; // Rethrow the exception to halt further processing or handle as appropriate
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred in ApplyATMTransferFee: {ex.Message}");
                throw; // Rethrow the exception to halt further processing or handle as appropriate
            }
        }
        private void ApplyATMWithdrawFee(int accountNumber)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Apply ATM withdraw fee
                var query = "UPDATE Account SET Balance = Balance - @Fee WHERE AccountNumber = @AccountNumber";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Fee", ATMWithdrawFee);
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    command.ExecuteNonQuery();
                }

                // Insert service charge transaction
                InsertServiceChargeTransaction(accountNumber, ATMWithdrawFee, "ATM withdrawal fee");
            }
        }

        private void InsertTransaction(SqlConnection connection, SqlTransaction transaction, char transactionType, int accountNumber, int? destinationAccountNumber, decimal amount, string comment)
        {
            
            const int maxCommentLength = 50; 
            if (!string.IsNullOrEmpty(comment) && comment.Length > maxCommentLength)
            {
                comment = comment.Substring(0, maxCommentLength);
            }

            var query = "INSERT INTO [Transaction] (TransactionType, AccountNumber, DestinationAccountNumber, Amount, Comment, TransactionTimeUtc) " +
                        "VALUES (@TransactionType, @AccountNumber, @DestinationAccountNumber, @Amount, @Comment, @TransactionTimeUtc)";

            using (var command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@TransactionType", transactionType);
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                command.Parameters.AddWithValue("@DestinationAccountNumber", destinationAccountNumber.HasValue ? (object)destinationAccountNumber.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@Comment", string.IsNullOrEmpty(comment) ? (object)DBNull.Value : comment);
                command.Parameters.AddWithValue("@TransactionTimeUtc", DateTime.UtcNow);

                command.ExecuteNonQuery();
            }
        }
        private void InsertServiceChargeTransaction(int accountNumber, decimal amount, string comment)
        {
            var transaction = new Transaction
            {
                TransactionType = 'S',
                AccountNumber = accountNumber,
                Amount = amount,
                Comment = comment,
                TransactionTimeUtc = DateTime.UtcNow
            };

            
            const int maxCommentLength = 50;
            if (!string.IsNullOrEmpty(transaction.Comment) && transaction.Comment.Length > maxCommentLength)
            {
                transaction.Comment = transaction.Comment.Substring(0, maxCommentLength);
            }

            _transactionManager.InsertTransaction(transaction);
        }
        public decimal GetAccountBalance(int accountNumber)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT Balance FROM Account WHERE AccountNumber = @AccountNumber";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    var result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        throw new Exception("Account not found.");
                    }

                    return (decimal)result;
                    return (decimal)result;
                }
            }
        }

        //Updates the transaction count column in Account database
        public async Task UpdateTransactionCounts()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT AccountNumber FROM Account;";
                var accounts = new List<int>();

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        accounts.Add(reader.GetInt32(0));
                    }
                }

                foreach (var accountNumber in accounts)
                {
                    // Excludes Deposit and Incoming Transfer transactions from the count
                    var transactionCountQuery = @"
                        SELECT COUNT(*) 
                        FROM [Transaction] 
                        WHERE AccountNumber = @AccountNumber
                          AND NOT (
                              TransactionType = 'D'
                              OR (TransactionType = 'T' AND DestinationAccountNumber = @AccountNumber)
                          );";

                    int transactionCount;

                    using (var countCommand = new SqlCommand(transactionCountQuery, connection))
                    {
                        countCommand.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        transactionCount = (int)await countCommand.ExecuteScalarAsync();
                    }

                    var updateQuery = @"UPDATE Account SET TransactionCount = @TransactionCount WHERE AccountNumber = @AccountNumber;";
                    using (var updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@TransactionCount", transactionCount);
                        updateCommand.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        await updateCommand.ExecuteNonQueryAsync();
                    }
                }

                Console.WriteLine("Updated transaction counts for accounts excluding free transactions.");
            }
        }


    }
}