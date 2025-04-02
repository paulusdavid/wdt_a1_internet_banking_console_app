using DataAccessLayerLib.Util.Managers;
using CommonLib.Data.Models;
using System;
using System.Collections.Generic;
using Assignment1.Util.Services;
using CommonLib.Util.Managers;
using Assignment1.Util.Menu;

namespace Assignment1.Util.Logic
{
    public class MainMenuLogic
    {
        private readonly LoginManager _loginManager;
        private readonly AccountManager _accountManager;
        private readonly CustomerManager _customerManager;
        private readonly TransactionManager _transactionManager;
        private readonly string _customerName;
        private readonly int _customerId;

        //Main Menu Logic Constructor
        public MainMenuLogic(string customerName, int customerId, LoginManager loginManager, AccountManager accountManager, CustomerManager customerManager, TransactionManager transactionManager)
        {
            _customerName = customerName;
            _customerId = customerId;
            _loginManager = loginManager;
            _accountManager = accountManager;
            _customerManager = customerManager;
            _transactionManager = transactionManager;

            //Calls menu print functionality
            IMenu menu = MainMenuUtils.CreateMenu(customerName);
            bool running = true;

            //Main menu case logic
            while (running)
            {
                Console.Clear();
                Console.WriteLine(menu.DisplayMenu());
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        HandleDeposit();
                        break;
                    case "2":
                        HandleWithdraw();
                        break;
                    case "3":
                        HandleTransfer();
                        break;
                    case "4":
                        HandleStatement();
                        break;
                    case "5":
                        running = false;
                        Logout();
                        break;
                    case "0":
                        Console.WriteLine("Program ending.");
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option, please try again.");
                        break;
                }
                if (running)
                {
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        //Logout user functionality
        private void Logout()
        {
            Console.WriteLine("You have been logged out.");
            Console.Clear();
            AuthenticationService.Login(_loginManager, _accountManager, _customerManager, _transactionManager).Wait();
        }

        //Deposit functionality
        private void HandleDeposit()
        {
            var userAccount = GetUserAccount();
            if (userAccount == null)
            {
                Console.WriteLine("Unable to retrieve the user account.");
                return;
            }

            var accountNumber = GetAccountNumber(userAccount);
            if (accountNumber == null)
            {
                Console.WriteLine("No valid account selected.");
                return;
            }

            Console.WriteLine("Enter the amount to deposit:");
            string amountInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(amountInput) || !decimal.TryParse(amountInput, out decimal amount))
            {
                Console.WriteLine("Invalid amount. Please try again.");
                return;
            }

            Console.WriteLine("Enter a comment for this transaction (optional):");
            string comment = Console.ReadLine();
            //Check validity of the inputted comment's length (MAX: 30 characters)
            if (!ValidateComment(comment)) return;

            try
            {
                _accountManager.Deposit(accountNumber.Value, amount, comment);
                Console.WriteLine("Deposit successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during deposit: {ex.Message}");
            }
        }

        //Withdraw amount from account
        private void HandleWithdraw()
        {
            var userAccount = GetUserAccount();
            if (userAccount == null)
            {
                Console.WriteLine("Unable to retrieve the user account.");
                return;
            }

            var accountNumber = GetAccountNumber(userAccount);
            if (accountNumber == null)
            {
                Console.WriteLine("No valid account selected.");
                return;
            }

            Console.WriteLine("Enter the amount to withdraw:");
            string amountInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(amountInput) || !decimal.TryParse(amountInput, out decimal amount))
            {
                Console.WriteLine("Invalid amount. Please try again.");
                return;
            }

            Console.WriteLine("Enter a comment for this transaction (optional):");
            string comment = Console.ReadLine();
            //Check validity of the inputted comment's length (MAX: 30 characters)
            if (!ValidateComment(comment)) return;

            try
            {
                _accountManager.Withdraw(accountNumber.Value, amount, comment);
                Console.WriteLine("Withdrawal successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during withdrawal: {ex.Message}");
            }
        }
        private int? GetAccountNumber(List<Account> accounts)
        {
            Console.WriteLine("Select the account type:");
            Console.WriteLine("1. Savings");
            Console.WriteLine("2. Checkings");

            string choice = Console.ReadLine();
            Account selectedAccount = null;

            switch (choice)
            {
                case "1":
                    selectedAccount = accounts.FirstOrDefault(a => a.AccountType == 'S');
                    break;
                case "2":
                    selectedAccount = accounts.FirstOrDefault(a => a.AccountType == 'C');
                    break;
                default:
                    Console.WriteLine("Invalid option, please try again.");
                    return null;
            }

            if (selectedAccount == null)
            {
                Console.WriteLine("Selected account type does not exist.");
                return null;
            }

            return selectedAccount.AccountNumber;
        }

        // Transfer money from current account
        private void HandleTransfer()
        {
            var userAccounts = GetUserAccount();
            if (userAccounts == null)
            {
                Console.WriteLine("Unable to retrieve the user account.");
                return;
            }

            var sourceAccountNumber = GetAccountNumber(userAccounts);
            if (sourceAccountNumber == null)
            {
                Console.WriteLine("No valid source account selected.");
                return;
            }

            Console.WriteLine("Enter the destination account number:");
            string destinationAccountNumberInput = Console.ReadLine();
            // Data validation check
            if (string.IsNullOrWhiteSpace(destinationAccountNumberInput) || !int.TryParse(destinationAccountNumberInput, out int destinationAccountNumber))
            {
                Console.WriteLine("Invalid destination account number. Please try again.");
                return;
            }
            else if (destinationAccountNumber == sourceAccountNumber)
            {
                Console.WriteLine("Provided numbers for sending and receiving account cannot be the same. Please try again.");
                return;
            }
            else if (destinationAccountNumber < 0)
            {
                Console.WriteLine("Account number source or destination cannot be the same. Please try again.");
                return;
            }
            else if (destinationAccountNumber == 0)
            {
                Console.WriteLine("Account number cannot be zero value. Please try again.");
                return;
            }
            else if (destinationAccountNumber < 1000 || destinationAccountNumber > 9999)
            {
                Console.WriteLine("Account number must be a 4-digit number. Please enter a valid 4-digit account number.");
                return;
            }

            Console.WriteLine("Enter the amount to transfer:");
            string amountInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(amountInput) || !decimal.TryParse(amountInput, out decimal amount))
            {
                Console.WriteLine("Invalid amount. Please try again.");
                return;
            }

            Console.WriteLine("Enter a comment for this transaction (optional):");
            string comment = Console.ReadLine();
            //Check validity of the inputted comment's length (MAX: 30 characters)
            if (!ValidateComment(comment)) return;

            try
            {
                _accountManager.Transfer(sourceAccountNumber.Value, destinationAccountNumber, amount, comment);
                Console.WriteLine("Transfer successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during transfer: {ex.Message}");
            }
        }

        // Show account's statement with pagination
        private void HandleStatement()
        {
            var userAccounts = GetUserAccount();
            if (userAccounts == null || !userAccounts.Any())
            {
                Console.WriteLine("No accounts found for the logged-in user.");
                return;
            }

            Console.WriteLine("Select the account number to view statement:");
            for (int i = 0; i < userAccounts.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {userAccounts[i].AccountNumber} ({(userAccounts[i].AccountType == 'S' ? "Savings" : "Checkings")})");
            }

            if (!int.TryParse(Console.ReadLine(), out int accountChoice) || accountChoice < 1 || accountChoice > userAccounts.Count)
            {
                Console.WriteLine("Invalid account choice. Please try again.");
                return;
            }

            int accountNumber = userAccounts[accountChoice - 1].AccountNumber;

            try
            {
                decimal balance = _accountManager.GetAccountBalance(accountNumber);
                var transactions = _transactionManager.GetTransactionsByAccountNumber(accountNumber)
                                                      .OrderByDescending(t => t.TransactionTimeUtc)
                                                      .ToList();

                Console.WriteLine($"Current Balance: {balance:C}");

                if (!transactions.Any())
                {
                    Console.WriteLine("No transactions found for this account.");
                    return;
                }

                const int pageSize = 4;
                int pageNumber = 0;
                bool paging = true;

                while (paging)
                {
                    var pageTransactions = transactions.Skip(pageNumber * pageSize).Take(pageSize).ToList();

                    if (!pageTransactions.Any())
                    {
                        Console.WriteLine("No more transactions to display.");
                        break;
                    }

                    Console.Clear();
                    Console.WriteLine($"Current Balance: {balance:C}");
                    Console.WriteLine("Transaction ID   | Type | Account Number   | Dest. Account   | Amount    | Transaction Time    | Comment");
                    Console.WriteLine(new string('-', 95));

                    foreach (var transaction in pageTransactions)
                    {
                        DateTime localTime = transaction.TransactionTimeUtc.ToLocalTime();
                        string formattedDate = localTime.ToString("dd/MM/yyyy hh:mm tt");

                        if (transaction.TransactionType == 'S')
                        {
                            // Display service charge transaction
                            Console.WriteLine($"{transaction.TransactionID,-16} | {transaction.TransactionType,-4} | {transaction.AccountNumber,-16} | {"-",15} | {transaction.Amount,-9:C} | {formattedDate,-20} | {transaction.Comment}");
                        }
                        else
                        {
                            // Display regular transaction
                            Console.WriteLine($"{transaction.TransactionID,-16} | {transaction.TransactionType,-4} | {transaction.AccountNumber,-16} | {transaction.DestinationAccountNumber,-15} | {transaction.Amount,-9:C} | {formattedDate,-20} | {transaction.Comment}");
                        }
                    }

                    Console.WriteLine("\nPress 'n' for next page, 'p' for previous page, or 'q' to quit:");

                    string input = Console.ReadLine().ToLower();


                    switch (input)
                    {
                        case "n":
                            if ((pageNumber + 1) * pageSize < transactions.Count)
                            {
                                pageNumber++;
                            }
                            else
                            {
                                Console.WriteLine("You are already on the last page.");
                            }
                            break;
                        case "p":
                            if (pageNumber > 0)
                            {
                                pageNumber--;
                            }
                            else
                            {
                                Console.WriteLine("You are already on the first page.");
                            }
                            break;
                        case "q":
                            paging = false;
                            break;
                        default:
                            Console.WriteLine("Invalid input. Please try again.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the statement: {ex.Message}");
            }
        }

        // Helper method to get the logged-in user's account
        private List<Account> GetUserAccount()
        {
            try
            {
                var accounts = _accountManager.GetAccountsByCustomerId(_customerId);

                if (accounts.Count > 0)
                {
                    return accounts;
                }
                else
                {
                    Console.WriteLine($"No account found for customer ID {_customerId}.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the user account: {ex.Message}");
                return null;
            }
        }

        // Helper method to validate comment length
        private bool ValidateComment(string comment)
        {
            const int maxCommentLength = 30; // Maximum length for comments
            if (comment.Length > maxCommentLength)
            {
                Console.WriteLine($"Comment is too long. Please limit your comment to {maxCommentLength} characters.");
                return false;
            }
            return true;
        }

    }
}