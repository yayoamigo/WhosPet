# Who's Pet Application

## Overview
This project is a web application for reporting lost pets. It is built using .NET C#, ASP.NET MVC, Web API, and follows Clean Architecture principles. It also uses JWT for login and registration, with authorization on the endpoints. Users can add pets, which are automatically assigned to the logged-in user, and create reports on those pets. Notifications are sent to all users when a report is created.

## Table of Contents
- [User Story](#user-story)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [Testing](#testing)
- [Postman Collections](#postman-collections)
- [Unit Tests](#unit-tests)

## User Story
As a user, I want to be able to create, read, update, and delete records so that I can manage my data effectively. Additionally, I want to be able to create an account and log in to the application to ensure my data is secure and only accessible to me.

## Project Structure
The project is organized according to Clean Architecture principles:
- **Core**: Contains domain entities and interfaces.
- **Infrastructure**: Contains data access implementations using ADO.NET.
- **API**: Contains ASP.NET Web API controllers.
- **UI**: Contains the presentation layer for the application.
- **Tests**: Contains unit tests for all components.

## Prerequisites
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Docker](https://www.docker.com/) (Optional, for containerizing the application)

## Setup Instructions
1. Clone the repository:
   ```bash
   git clone https://github.com/your-repo/whospet.git
   cd whospet
Open the solution in Visual Studio.

Update the connection string in appsettings.json to point to your local SQL Server instance:

"ConnectionStrings": {
  "WhosPet": "Server=your_server_name;Database=WhosPetDb;User Id=your_username;Password=your_password;"
} 
## Using Local SQL Server
-Install SQL Server if not already installed.

-Create a new database named WhosPetDb.

-Run the provided SQL scripts to create the necessary tables
```sql
CREATE TABLE RefreshToken (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(MAX) NOT NULL,
    jwtId NVARCHAR(255) NOT NULL,
    IsUsed BIT NOT NULL,
    IsRevoked BIT NOT NULL,
    ExpireDate DATETIME2 NOT NULL,
    CreatedDate DATETIME2 NOT NULL
);
CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    UserName NVARCHAR(256) NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL,
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(MAX) NULL,
    PhoneNumberConfirmed BIT NOT NULL,
    TwoFactorEnabled BIT NOT NULL,
    LockoutEnd DATETIMEOFFSET NULL,
    LockoutEnabled BIT NOT NULL,
    AccessFailedCount INT NOT NULL
);

CREATE TABLE AspNetRoles (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL
);

CREATE TABLE AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetUserClaims (
    Id INT NOT NULL PRIMARY KEY IDENTITY,
    UserId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetRoleClaims (
    Id INT NOT NULL PRIMARY KEY IDENTITY,
    RoleId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetUserLogins (
    LoginProvider NVARCHAR(450) NOT NULL,
    ProviderKey NVARCHAR(450) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE AspNetUserTokens (
    UserId NVARCHAR(450) NOT NULL,
    LoginProvider NVARCHAR(450) NOT NULL,
    Name NVARCHAR(450) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    PRIMARY KEY (UserId, LoginProvider, Name),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

-- Table for UserProfile
CREATE TABLE UserProfiles (
    Email NVARCHAR(256) NOT NULL PRIMARY KEY,
    Name NVARCHAR(MAX) NOT NULL,
    Surname NVARCHAR(MAX) NOT NULL,
    City INT NOT NULL,
    Address NVARCHAR(MAX) NOT NULL,
    FOREIGN KEY (Email) REFERENCES AspNetUsers(Email) ON DELETE CASCADE
);

-- Table for Pets
CREATE TABLE Pets (
    Id INT NOT NULL PRIMARY KEY IDENTITY,
    Name NVARCHAR(MAX) NOT NULL,
    Type INT NOT NULL,
    Breed NVARCHAR(MAX) NOT NULL,
    Color NVARCHAR(MAX) NOT NULL,
    City NVARCHAR(MAX) NOT NULL,
    Age INT NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Image NVARCHAR(MAX) NOT NULL,
    UserId NVARCHAR(450) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES UserProfiles(Email) ON DELETE CASCADE
);

-- Table for LostPetReports
CREATE TABLE LostPetReports (
    Id INT NOT NULL PRIMARY KEY IDENTITY,
    UserId NVARCHAR(450) NOT NULL,
    PetId INT NOT NULL,
    PetName NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Date DATETIME NOT NULL,
    City NVARCHAR(MAX) NOT NULL,
    Longitude FLOAT NOT NULL,
    Latitude FLOAT NOT NULL,
    Image NVARCHAR(MAX) NOT NULL,
    IsFound BIT NOT NULL,
    IsActive BIT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES UserProfiles(Email) ON DELETE CASCADE,
    FOREIGN KEY (PetId) REFERENCES Pets(Id) ON DELETE CASCADE
);

-- Table for Notifications
CREATE TABLE Notifications (
    Id INT NOT NULL PRIMARY KEY IDENTITY,
    UserId NVARCHAR(450) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Timestamp DATETIME NOT NULL,
    IsRead BIT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES UserProfiles(Email) ON DELETE CASCADE
);
```
## Start the API and UI project

When the project starts a user will be created, you can use it to login as Admin
email: yayon@example.com
password: 123

## Testing
For testing the repositories you would need to create a new DB called WhosPetTest, with the previous tables, and replace the connection string on each test

##PostMan
You can find 3 postman files at the root of this project to test the endpoints 
