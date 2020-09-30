# Overview

Student Bank is a mock core banking application that allows students to interact with Checking and Savings accounts, a mock stock market, and more.

# Environment Variables

The following environment variables or app settings must be specified before launching the server:

* `AppConfig__Secret` should be a long, private string used to generate JWT tokens.  Changing this invalidates tokens.

# Generate Migrations

Migrations and database contexts are stored in a separate assembly than the server to enable easier sharing between servers, utilities
and other applications in general.  This adds a little bit more complexity to generating migrations, in addition to two different
database technologies being supported:

    cd jahndigital.studentbank.dal
    dotnet ef migrations add -s ../jahndigital.studentbank.server -c SqliteDbContext -o Migrations/sqlite {NAME}
    dotnet ef migrations add -s ../jahndigital.studentbank.server -c AppDbContext -o Migrations/mssql {NAME}
