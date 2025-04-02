# wdt_a1_internet_banking_console_app
Internet Banking console app for WDT_A1

[REDACTED]
# s3XXXXXX-s3XXXXXX-a1
Internet Banking console app for WDT_A1
Developed collaboratively by GROUP 3
-Created by: Michael Whyte (s3XXXXXX)
             Paulus David (s3XXXXXX)

Trello milestone link: 
https://trello.com/b/DoY7AQIk/wdta1-internet-banking-console-based-application

Github link: [PRIVATE]
https://github.com/rmit-wdt-sp2-2024/s3851481-s3675333-a1

Azure SQL Server Account:
        
        Server:	rmit.australiaeast.cloudapp.azure.com
        Database: s3851481_a1
        Encrypt: Mandatory
        Trust Server Certificate: True

How to run:
    
    Requirements
        a) .NET SDK: Ensure you have the .NET 7.0 SDK installed on your system.
        b) Visual Studio installed (preferrably 2022 or above builds).
        c) Active internet connection

    Steps to Run the Project
        
        1) Clone/Download the Project

            a) Obtain the project files from the provided source (Github).
        
        2) Open the Project

            a) Open Visual Studio.
            b) Select 'Open a project or solution'.
            c) Navigate to the directory where you have saved the project.
            d) Select the project file (usually with a .sln or .csproj extension) and open it.
            
        3) Restore NuGet Packages

            a) Right-click on the solution in Solution Explorer.
            b) Select 'Restore NuGet Packages' to ensure all dependencies are properly installed.

        4) Build the Project

            a) Build the project by right-clicking on the solution and selecting 'Build Solution'.
        
        5) Run the Project

            a) Start the application by pressing F5 or clicking the 'Start' button in Visual Studio.
            b) This will launch the console application, where you can interact according to the project's functionality.

Troubleshoot Errors

    a) Connection Issues: Make sure you have an active internet connection so program can communicate to the server properly and function accordingly.
    b) Build Errors: Check if all NuGet packages are restored and all project references are correctly configured.
    c) NET Version = .NET 7.0 is where this was designed anything lower may impact compatibility in runtime.

Design and Implementation:
** Design Pattern #1 - Dependency Injection
    Dependency Injection is a fundamental design pattern used to manage and decouple dependencies between objects. Rather than objects creating dependencies themselves, these dependencies are provided externally: via a constructor, a factory, or a service locator.

    The primary purpose of implementing DI in the banking console app project is to reduce the coupling between components of an application, thus increasing its modularity and making it easier to manage, test, and maintain. 

    Some advantages of DI include:
        Improved Testability: By injecting dependencies, it becomes much easier to replace them with mock objects during testing.
        Increased Clarity: Central management of dependencies reduces redundancy and leads to clearer, more concise code.
        Enhanced Flexibility: Changes to dependencies or their configurations need only be updated in the service configuration, not in the dependent classes.

    Implementation of the Design Pattern
    Dependency Injection requires 'Microsoft.Extensions.DependencyInjection' library to work.

    Service Registration: This occurs in the ConfigureServices method within the Program.cs file. Here, services such as CustomerManager, LoginManager, TransactionManager, and AccountManager are registered with specific lifetimes. This setup dictates how and when instances of these classes are created and shared.

    Service Resolution: Occurs in the Main method of Program.cs, where instances of these services are requested from the DI container. This method of obtaining services ensures that the requesting code is decoupled from the creation logic of these services.
    
    Specific code implementation of DI in the project:

        Program.cs:

            a) ConfigureServices method: 
            Here, services mentioned above are added to the ServiceCollection. This method leverages lambdas and factory patterns to configure the instantiation of services with required dependencies.

            b) Main method: Demonstrates how to resolve these services to perform operations like data fetching, saving, and user authentication, ensuring that each component operates with its required dependencies without needing to manage their lifecycle.

    Managers and Services:

        Other classes such as CustomerManager and AccountManager are constructed with dependencies passed into their constructors, which are defined and resolved via DI. This ensures that these managers are easy to replace and do not control how their dependencies are created.

        Scalability and Maintenance: Using DI ensures the application architecture to be scalable and easier to maintain. As new dependencies or services become necessary, they can be integrated without disrupting existing code significantly.

** Design Pattern #2 - Data Transfer Object (DTO)
    Data Transfer Objects (DTOs) are crucial in software architecture, particularly in scenarios involving data exchange across different system layers or network boundaries. They encapsulate data attributes into plain objects without business logic, facilitating efficient data transmission and reducing the number of calls needed between clients and servers or within different application layers.

    Purpose and benefits of DTO in the project
    DTOs serves to streamline data handling processes in applicaton by bundling data together, in this case having DTOs for Account, Customer, Login and Transactions minimising the need for multiple calls to fetch or send data.

    The key benefits of using DTO in the project:
        *   Network Efficiency: DTOs consolidate data requests and responses, which is particularly beneficial in reducing network load and improving application responsiveness.

        *   Ease of Maintenance: By separating the data representation used externally from the internal working models, DTOs make it easier to maintain and modify the application without extensive retesting or refactoring.

        *   Decoupling: They help decouple the presentation layer data from the business services, ensuring that changes in the database or business logic have minimal impacts on the client-facing sides of the application.

    Implementation Details:
        DTO Usage in Web Service Call:
            * In the CustomerWebService.cs within the Util.Services folder, DTOs are employed to fetch and send data from an external web service. The received data is then mapped to the domain models or entities before being processed or persisted.

        Files and Locations:
            DTOs: Located in the CommonLib.Util.DTOs folder, which contains all DTO classes AccountDto.cs, CustomerDto.cs, LoginDto.cs and TransactionsDto.cs files that are used in the project.

            Usage: DTOs are mainly used in the service layer and are important for data exchange between the database layer and web services.

** Class library
In the design of the Assignment1 project, two distinct class libraries, CommonLib and DataAccessLayerLib, have been strategically employed to promote modularity, reusability, and maintainability of the code. This approach aligns with modern software architecture principles, facilitating clear separation of concerns and enhanced scalability.
      
      Details of Implementation:
  
          A) CommonLib
                Purpose: This library serves as a central repository for all shared domain models and Data Transfer Objects (DTOs). By abstracting these models into a separate library, CommonLib ensures that core data structures are reusable across different parts of the application, including future expansions or external applications.
                Contents:
                    * Models: Account, Customer, Login, Transaction   these classes represent the essential data structures used throughout the application.
                    * DTOs: AccountDto, CustomerDto, LoginDto, TransactionDto   these are used for data transfer operations, particularly in API consumption and response formation.
          
          B) DataAccessLayerLib
                Purpose: This library encapsulates all data access logic, abstracting the complexities of database interactions from the business logic contained in Assignment1. It utilizes CommonLib for its domain models, ensuring that any changes to data structures are consistently propagated throughout all database interactions.
                Contents:
                Managers: AccountManager, CustomerManager, LoginManager, TransactionManager   these classes manage all CRUD operations related to their respective models, offering a clean API for the main application to interact with the database without direct SQL exposure.
        
        Benefits of class libraries implementation:
            * Encapsulation and Separation of Concerns: Each library encapsulates specific responsibilities:
                a) CommonLib handles the definition of data structures.
                b) DataAccessLayerLib manages all database operations. 
                This separation enhances maintainability and allows changes in one area 

            * Ease of Maintenance: Bugs can be isolated and fixed in specific libraries without the risk of affecting other components. Updates or improvements to the data access layer can be made independently of the business logic and UI layers.
            * Reusability: Both libraries can be reused in different parts of the application or even in different projects within the same organization. For example, CommonLib can be used in other backend services that require the same data models.
            * Scalability: As the project grows, new functionalities can be added more smoothly. For instance, adding new features or services often requires extending the data models or data access layers. Having these components in separate libraries makes such extensions more manageable and less prone to errors.

** Required keyword
    The required keyword is strategically implemented in the console-based banking application to enhance data integrity and enforce the initialization of critical properties directly at compile-time. 

    Such advantages of implementing "required" keyword are:

        * Enforcement of Non-nullability: The required keyword strengthens the integrity of our data model by guaranteeing that essential properties are not null at runtime. This is crucial for operations that depend on these properties to perform data insertion when checking the database's emptiness state and saving user's financial state of their account.
    
        * Compile-time Safety: By leveraging C# s capability to enforce initializations at compile time, it significantly reduce the risk of null reference exceptions, which are common sources of runtime errors in .NET applications.

        * Code Clarity and Reliability: The use of the required keyword clarifies the developer s intentions and makes the codebase easier to understand and maintain. It explicitly indicates which properties must be set for an object to be considered valid, thus enhancing code reliability.

    Files and Code Locations:

     -Location -> CommonLib.Data.Models

      Account.cs: The AccountNumber, AccountType, CustomerID, and Balance properties in the Account class are declared with the required keyword to ensure that these critical banking details are fully initialized upon object creation.

      Customer.cs: Similar usage in the Customer class ensures that CustomerID, Name, Address, City, and PostCode are initialized, reflecting their mandatory nature in maintaining customer information integrity.

      Login.cs: In the Login class, LoginID, CustomerID, and PasswordHash utilize the required keyword to enforce the initialization, crucial for security and identification purposes.

      Transaction.cs: For transactions, TransactionID, TransactionType, AccountNumber, Amount, and TransactionTimeUtc are marked as required to ensure all transaction records are complete and valid at the time of creation.

      
     -Location -> CommonLib.Util.DTOs

      AccountDto: In AccountDto.cs, the AccountNumber and AccountType properties are marked with required. This ensures that every account has these fundamental properties set upon instantiation, which are vital for identifying accounts and processing transactions.

      CustomerDto: Located in CustomerDto.cs, properties such as CustomerID, Name, Address, City, and PostCode are required. This enforces a complete customer profile necessary for account management and service personalization.

      LoginDto: In LoginDto.cs, both LoginID and PasswordHash use the required keyword. These properties are essential for the security and functionality of the login process, ensuring that each login attempt are entered with valid credentials.

      TransactionDto: Found in TransactionDto.cs, the Amount and TransactionTimeUtc are declared as required. These properties are critical for recording transactions accurately, where Amount must be initialized to ensure financial consistency and TransactionTimeUtc captures the exact time of transaction for historical and auditing purposes.

      These files are part of the CommonLib.Data.Models and CommonLib.Util.DTOs folder and demonstrate the effective use of the required keyword to safeguard data consistency across the application.


** Asynchronous Implementation
    The asynchronous methods are implemented to handle operations that involve external data access or services which can introduce latency, such as database interactions or external API calls. 
    
    The key benefits provided by asynchronous programming in the project include:
        * Enhanced Responsiveness: Asynchronous methods prevent the UI from becoming unresponsive. This is crucial in maintaining a smooth user interaction experience, where the system continues to be responsive to user commands even while performing long-running operations.
    
        * Improved Scalability: The non-blocking nature of asynchronous methods allows the application to scale up efficiently, handling a larger number of operations concurrently without a linear increase in resource consumption.

        * Resource Efficiency: With asynchronous operations, the application can handle more tasks with fewer threads. This is because threads that would otherwise be blocked waiting for I/O operations are freed up to perform other tasks. This approach maximizes resource utilization and throughput.

    Implementation of async in the project: Program.cs, CustomerWebService.cs and AuthenticationService.cs where async and await keyword are implemented for any interaction made from app to server and executing the main commands.

@RMIT UNIVERSITY - Web Development Technologies (2437)
