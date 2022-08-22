# Overview

Student Bank is a mock core banking application that allows students to interact with Checking and Savings accounts, a mock stock market, and more.

# Getting Started (Development)

Copy and edit the `appsettings.json*.dist` files:

```shell
    cp src/WebApi/appsettings.json.dist src/WebApi/appsettings.json
    cp src/WebApi/appsettings.Development.json.dist src/WebApi/appsettings.Development.json
```

Be sure to edit the ConnectionStrings__Default setting if you are not using `docker-compose`.

Next, run `docker-compose up` if you don't already have an MSSQL server to use for development.

Ensure that the dotnet development certificates are trusted by your machine by running `dotnet dev-certs https --trust`.  You'll be prompted to import them.

Finally, run `dotnet run --project src/WebApi` to fetch nuget packages and start the app.

# Development Tips

## Secret needed for JWT

>**TIP:** By default, a secret is stored in the `src/WebApi/launchSettings.json` file.  It is only used during development and an error will be thrown if no secret is provided when running the application in Release mode.

* `AppConfig__Secret` should be a long, private string used to generate JWT tokens.  Changing this invalidates tokens.

## Migrations

To setup migrations, you must install the Entity Framework command line tools:

```shell
dotnet tool install --global dotnet-ef
dotnet ef
```

### Generating Migrations

Migrations and database contexts are stored in a separate assembly than the server to enable easier sharing between servers, utilities
and other applications in general.  This adds a little bit more complexity to generating migrations, in addition to two different
database technologies being supported.

```shell
    cd src/Infrastructure
    dotnet ef migrations add {NAME}
```

# Deploying to Production in Azure

This code was designed to run in Azure, however; it can be run anywhere.  This guide is intended to walk you through how to deploy the app using Azure App Service.

## Azure SQL Configuration

Begin by creating a new Azure SQL database.  Choose an appropriate pricing tier, though it is recommended to use the **Basic** tier initially for this workload to save costs and grow as needed.

Next, grant the *Managed Identity* for your Azure App Service App the `Reader` role on the SQL Server Instance.  This can be done using the **Access Control (IAM)** option on the Azure SQL Server blade.

Optionally, it is highly recommended to restrict access to the Azure SQL Server instance via the **Security > Networking** option in the SQL Server blade.  Note the database name and server for later use when configuring the App Service App.

## Azure App Service Configuration

> **Note:** This application has not been tested against the F1 free tier of App Service.  It may run out of compute or memory and crash under load.

Begin by creating a new Azure App Service on a Linux App Service Plan.  The **B1** tier is recommended to begin with to save costs.

Ensure that a System assigned Managed Identity is enabled for use with Azure SQL Database.

Optionally, but recommended: Configure a custom domain and SSL, however; the app will function just fine using the URL Microsoft assigns to the App Service.  Note that using the App Service Managed Certificate can save costs and the certificate will automatically renew.

Add the following Configuration entries using the **Configuration** blade of the App Service:

| Name | Comment |
| ---- |---------|
| AllowedOrigins__`{0,1,2,...}` | Used for CQRS.  One or more URLs allowed to access the site.  E.g. `https://fluffy-bunny-00001.azurestaticapps.net` |
| AppConfig__Secret | A unique string used to sign JWT requests.  Recommended to generate two GUIDs and concatenate them. |
| AppConfig__TokenLifetime | The amount of time in minutes JWT tokens are valid for.  The client must refresh the token once it expires.  Default: 15 minutes. |
| ConnectionStrings__Default | The database connection string.  E.g. using Managed Identity: `Server=my-db.database.windows.net,1433;Database=prod_studentbank;UID=a;Authentication=Active Directory Interactive` |

On the **Configuration > General settings** page, ensure the following is set:

- **Stack:** .NET
- **Major version:** .NET 6
- **Minor version:** .NET 6 (LTS)
- **Always on:** On
- **HTTPS Only:** On
- **Minimum TLS Version:** 1.2

## Azure SQL Database Configuration

> **Tip:** You can look up the name of your Managed Identity by searching Azure Active Directory for the GUID found in the App Service App's Identities blade option.

Next, [Follow the directions from Microsoft](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/tutorial-windows-vm-access-sql) to enable Azure AD authentication if it's not already enabled.

Finally, create a login and grant it permissions to the database.  Here's an example on how to create a user record and assign it db_owner permissions on a database:

```sql
    CREATE USER [studentbank] FROM EXTERNAL PROVIDER
    ALTER ROLE db_owner ADD MEMBER [studentbank]
```

Where `studentbank` is the name of your App Service App.

## Start the App

Click the **Start** button on the **Overview** page of the App Service App to run it.  Check the **Log Stream** for any errors that might have occurred during startup.  If everything is working correctly, you can navigate to `https://my-site.domain.tld/graphql` (where `my-site.domain.tld` is the URL of your site from Azure App Service) to be presented with the BananaCakePop GraphQL Web IDE.
