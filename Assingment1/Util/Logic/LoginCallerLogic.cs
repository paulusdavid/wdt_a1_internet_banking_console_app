using Assignment1.Util.Services;
using CommonLib.Util.Managers;
using DataAccessLayerLib.Util.Managers;
using System;
using System.Threading.Tasks;

namespace Assignment1.Util.Logic
{
    public class LoginCallerLogic
    {
        public static async Task ExecuteLogin(LoginManager loginManager, AccountManager accountManager, CustomerManager customerManager, TransactionManager transactionManager)
        {
            await Task.Delay(2000);  // Simulated delay for user experience
            await AuthenticationService.Login(loginManager, accountManager, customerManager, transactionManager);
        }
    }
}
