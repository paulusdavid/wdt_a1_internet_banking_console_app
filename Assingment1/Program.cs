using Assignment1.Util.Logic;
using CommonLib.Util.Managers;
using DataAccessLayerLib.Util.Managers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assignment1
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            try
            {
                var customerManager = serviceProvider.GetRequiredService<CustomerManager>();
                var loginManager = serviceProvider.GetRequiredService<LoginManager>();
                var transactionManager = serviceProvider.GetRequiredService<TransactionManager>();
                var accountManager = serviceProvider.GetRequiredService<AccountManager>();

                await DatabaseCreationLogic.CreateAllTables(customerManager, loginManager, accountManager, transactionManager);
                await DatabaseInsertionLogic.FetchAndSaveData(customerManager, loginManager, accountManager, transactionManager);
                await LoginCallerLogic.ExecuteLogin(loginManager, accountManager, customerManager, transactionManager);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<CustomerManager>(_ => new CustomerManager(connectionString));
            services.AddTransient<LoginManager>(_ => new LoginManager(connectionString));
            services.AddTransient<TransactionManager>(_ => new TransactionManager(connectionString));
            services.AddTransient<AccountManager>(provider =>
                new AccountManager(connectionString, provider.GetRequiredService<TransactionManager>()));
        }
    }
}
