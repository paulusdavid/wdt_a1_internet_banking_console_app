using CommonLib.Util.Managers;
using System;
using System.Threading.Tasks;
using Assignment1.Util.Services;
using DataAccessLayerLib.Util.Managers;

namespace Assignment1.Util.Logic
{
    public class DatabaseInsertionLogic
    {
        public static async Task FetchAndSaveData(CustomerManager customerManager, LoginManager loginManager, AccountManager accountManager, TransactionManager transactionManager)
        {
            await CustomerWebService.GetAndSaveCustomers(customerManager, loginManager, accountManager, transactionManager);
            Console.WriteLine("Data has been fetched and saved to the database.");
        }
    }
}
