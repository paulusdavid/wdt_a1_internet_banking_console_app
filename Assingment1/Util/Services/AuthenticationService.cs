using System;
using System.Threading.Tasks;
using SimpleHashing.Net;
using DataAccessLayerLib.Util.Managers;
using Assignment1.Util.Menu;
using CommonLib.Data.Models;
using CommonLib.Util.Managers;

namespace Assignment1.Util.Logic
{
    // Provides authentication services to validate users and grant access.
    public static class AuthenticationService
    {
        /*
         * Handles user login attempts asynchronously.
         */

        public static async Task Login(LoginManager loginManager, AccountManager accountManager, CustomerManager customerManager, TransactionManager transactionManager)
        {
            while (true)
            {
                Console.Write("Enter LoginID: ");
                string loginId = Console.ReadLine(); // User inputs their login ID

                Console.Write("Enter Password: ");
                string password = ReadPassword(); // Securely read password input

                // Attempt to retrieve and verify the login credentials
                var login = await loginManager.GetLogin(loginId);
                if (login != null && new SimpleHash().Verify(password, login.PasswordHash))
                {
                    Console.WriteLine("Login successful!");

                    // Fetch customer details to get the name
                    var customer = await customerManager.GetCustomerById(login.CustomerID);

                    // Pass all required parameters
                    new MainMenuLogic(customer.Name, login.CustomerID, loginManager, accountManager, customerManager, transactionManager);
                    break; // Exit the login loop on successful authentication
                }
                else
                {
                    // If login fails, display an error message and repeat the loop
                    Console.WriteLine("Invalid LoginID or Password. Please try again.");
                }
            }
        }

        /* 
         * Securely reads a password from console input, masking the characters typed by the user.
         * The password entered by the user as a string.
         */
        private static string ReadPassword()
        {
            var password = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true); // Read each key pressed, without displaying it
                key = keyInfo.Key;

                // Handle backspace (allow user to correct input)
                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b"); // Remove the last character in the console
                    password = password[0..^1]; // Remove the last character from the password string
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");  // Mask the user input
                    password += keyInfo.KeyChar; // Append the character to the password string
                }
            } while (key != ConsoleKey.Enter); // Continue reading input until 'Enter' is pressed
            Console.WriteLine(); // Move the cursor to the next line
            return password;
        }
    }
}
