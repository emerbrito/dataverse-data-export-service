This solution aims to provide an alternative to the [Microsoft Data Export Service (DES)][1] which reached end-of-life in November 2022.
The database created by this service is fully compatible with the one from the deprecated Microsoft’s DES. To move your client (e.g.: Power BI) to the new database all you need is to swap your connection string.

![basic function app implementation](https://github.com/emerbrito/dataverse-data-export-service/blob/main/images/basic-functionapp-diagram.png)

While the source code includes an implementation of this service as an Azure function and the documentation may be focusing on this implementation, the actual functionality is contained in its own .net 6 project making it possible to implement it as a command line tool, Windows service, docker container and so on.

## How it works

The service uses the built-in table (entity) change tracking capability to synchronize data. Change tracking allows the service to reliably retrieve only the changes between the last and current call to the dataverse API.
The following tasks are performed for each synchronized table (entity) every time the service runs:

1. Determine whether change tracking is enabled and if not, enable it.
2.Create the SQL table if it not already exists or compare the schema of an existing table to most up to date metadata and update the SQL table if necessary (drop, create or update columns).
3.Use the last data token (stored in the SQL database) to determine which data to synchronize. If there is no data token it is most likely the first call to the API and all data from the table will be copied.
4.Store the current data token which will be used in the next call to the API.

## Cloning the repo and running the service locally

After cloning the repository, make sure entered the minimum required configuration before you run it locally (see the [Configuration](#configuration) for more details).
For help running a function locally see the [Run the function locally][3] section of the "[Quickstart: Create your first C# function in Azure using Visual Studio][2]" guide.

## Deploying the Azure Function

After cloning the repository, you can run it locally by entering the minimum required configuration (see the configuration section for more details) 

For help publishing an Azure function see [Publish the project to Azure][4] section of the "[Quickstart: Create your first C# function in Azure using Visual Studio][2]" guide.

## Configuration

If you need help using the portal to enter your application settings, see the section [Work with application settings][5] of the [Manage your function app][5] guide.

While working locally, you can enter the same settings in your local.settins.json

### Application Settings

| Key  | Description  |
| ---- | ------------ |
| ClientId | (Required) The application ID used to connect to the dataverse. |
| ClientSecret | (Required) The application secret used to connect to the dataverse. |
| DataverseInstanceUrl | (Required) The dataverse instance URL.
| DataverseQueryPageSize | (Optional) The page size used when querying the dataverse API. Only change this value if the records in some of your tables are too large and you are experiencing SQL errors. Default is 5000. |
| ScheduleCronExpression | (Optional). A cron expression indicating the timer interval or how often the function will run. More info on cron expressions can be found [in the timer trigger documentation][2]. The default value is 15 minutes. |
| SqlCommandTimeoutSeconds | (Optional) The time out applyed to SQL Commands. Default is 30 seconds. |
| RetryLinearBackoffInitialDelaySeconds | (Optional) The initial delay applied to the exponential back-off back off retry policy. Default is 30 seconds. |
| RetryLinearBackoffRetryCount | (Optional) Number of attempts made by the retry policy. Default is 6.
| StoreConnectionString | (Required) The SQL database connection string (use the connection string section of the app settings or in the Azure portal). |

### Initializing the SQL Database

Every time the service runs it will make an attempt to initialize the database in case it is empty (have no tables at all).

### Synchronized Tables (Entities)

To register Dataverse tables (entities) to for synchronization, open the SQL database and update the table "_SynchronizedTables".
Enter the table (entity) logical name and "true" for the enabled column.

![adding new tables for synchronization](https://github.com/emerbrito/dataverse-data-export-service/blob/main/images/add-new-tables.jpg)

[1]: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/data-export-service
[2]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-create-your-first-function-visual-studio
[3]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-create-your-first-function-visual-studio?tabs=in-process#run-the-function-locally
[4]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-create-your-first-function-visual-studio?tabs=in-process#publish-the-project-to-azure
[5]: https://learn.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings?tabs=portal#settings
