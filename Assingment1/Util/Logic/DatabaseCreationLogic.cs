using CommonLib.Util.Managers;
using DataAccessLayerLib.Util.Managers;
using System;
using System.Threading.Tasks;

namespace Assignment1.Util.Logic
{
    public class DatabaseCreationLogic
    {
        public static async Task CreateAllTables(CustomerManager customerManager, LoginManager loginManager, AccountManager accountManager, TransactionManager transactionManager)
        {
            await CreateCustomerTable(customerManager);
            await CreateLoginTable(loginManager);
            await CreateAccountTable(accountManager);
            await CreateTransactionTable(transactionManager);
        }

        private static async Task CreateCustomerTable(CustomerManager customerManager)
        {
            try
            {
                await customerManager.CreateCustomerTableIfNotExists();
                Console.WriteLine("Customer table created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Customer table: {ex.Message}");
                throw;
            }
        }

        private static async Task CreateLoginTable(LoginManager loginManager)
        {
            try
            {
                await loginManager.CreateLoginTableIfNotExists();
                Console.WriteLine("Login table created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Login table: {ex.Message}");
                throw;
            }
        }

        private static async Task CreateAccountTable(AccountManager accountManager)
        {
            try
            {
                await accountManager.CreateAccountTableIfNotExists();
                Console.WriteLine("Account table created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Account table: {ex.Message}");
                throw;
            }
        }

        private static async Task CreateTransactionTable(TransactionManager transactionManager)
        {
            try
            {
                await transactionManager.CreateTransactionTableIfNotExists();
                Console.WriteLine("Transaction table created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Transaction table: {ex.Message}");
                throw;
            }
        }
    }
}
