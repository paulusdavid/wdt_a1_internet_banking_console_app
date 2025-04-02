using Newtonsoft.Json;
using DataAccessLayerLib.Util.Managers;
using CommonLib.Data.Models;
using Assignment1.Util.DTOs;
using CommonLib.Util.Managers;
using System;

namespace Assignment1.Util.Services
{
        public static class CustomerWebService
        {
                public static async Task GetAndSaveCustomers(CustomerManager customerManager, LoginManager loginManager, AccountManager accountManager, TransactionManager transactionManager)
                {
                        const string Url = "https://coreteaching01.csit.rmit.edu.au/~e103884/wdt/services/customers/";

                        try
                        {
                                // Contact web service.
                                using var client = new HttpClient();
                                var json = await client.GetStringAsync(Url); // Asynchronous call

                                // Convert JSON into objects.
                                var customerDtos = JsonConvert.DeserializeObject<List<CustomerDto>>(json);

                                // Insert or update into database.
                                foreach (var customerDto in customerDtos)
                                {
                                        try
                                        {
                                                // Check if customer already exists
                                                if (customerManager.Exists(customerDto.CustomerID))
                                                {
                                                        Console.WriteLine($"Customer with ID {customerDto.CustomerID} already exists. Skipping insertion.");
                                                }
                                                else
                                                {
                                                        // Create Customer object
                                                        var customer = new Customer
                                                        {
                                                                CustomerID = customerDto.CustomerID,
                                                                Name = customerDto.Name,
                                                                Address = customerDto.Address,
                                                                City = customerDto.City,
                                                                PostCode = customerDto.PostCode
                                                        };

                                                        // Insert Customer
                                                        await customerManager.InsertCustomer(customer);
                                                        Console.WriteLine($"Inserted Customer with ID: {customer.CustomerID}");

                                                        // Insert Accounts and Transactions
                                                        foreach (var accountDto in customerDto.Accounts)
                                                        {
                                                                try
                                                                {
                                                                        var account = new Account
                                                                        {
                                                                                AccountNumber = accountDto.AccountNumber,
                                                                                AccountType = accountDto.AccountType,
                                                                                CustomerID = customer.CustomerID,
                                                                                Balance = accountDto.Transactions.Sum(t => t.Amount)
                                                                        };

                                                                        // Insert Account
                                                                        await accountManager.InsertAccount(account);
                                                                        Console.WriteLine($"Inserted Account with Number: {account.AccountNumber}");

                                                                        // Insert Transactions
                                                                        foreach (var transactionDto in accountDto.Transactions)
                                                                        {
                                                                                try
                                                                                {
                                                                                        var transaction = new Transaction
                                                                                        {
                                                                                                TransactionType = 'D',
                                                                                                AccountNumber = account.AccountNumber,
                                                                                                Amount = transactionDto.Amount,
                                                                                                Comment = transactionDto.Comment,
                                                                                                TransactionTimeUtc = DateTime.ParseExact(transactionDto.TransactionTimeUtc, "dd/MM/yyyy hh:mm:ss tt", null)
                                                                                        };

                                                                                        // Insert Transaction
                                                                                        transactionManager.InsertTransaction(transaction);
                                                                                        Console.WriteLine($"Inserted Transaction for Account Number: {account.AccountNumber}");
                                                                                }
                                                                                catch (Exception ex)
                                                                                {
                                                                                        Console.WriteLine($"Error inserting transaction for Account Number {account.AccountNumber}: {ex.Message}");
                                                                                        // Handle or log the exception
                                                                                }
                                                                        }
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                        Console.WriteLine($"Error inserting account {accountDto.AccountNumber}: {ex.Message}");
                                                                        // Handle or log the exception
                                                                }
                                                        }

                                                        // Insert Login
                                                        var login = new Login
                                                        {
                                                                LoginID = customerDto.Login.LoginID,
                                                                PasswordHash = customerDto.Login.PasswordHash,
                                                                CustomerID = customer.CustomerID
                                                        };
                                                        await loginManager.InsertLogin(login);
                                                        Console.WriteLine($"Inserted Login for Customer ID: {customer.CustomerID}");
                                                }
                                        }
                                        catch (Exception ex)
                                        {
                                                Console.WriteLine($"Error processing customer {customerDto.CustomerID}: {ex.Message}");
                                                // Handle or log the exception
                                        }
                                }
                        }
                        catch (Exception ex)
                        {
                                Console.WriteLine($"Error fetching customers from web service: {ex.Message}");
                                // Handle or log the exception
                        }
                }

        }
}
