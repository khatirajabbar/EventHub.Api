# EventHub API 🎟️

EventHub is a robust, secure, and scalable ASP.NET Core RESTful API designed for managing events, organizers, tickets, and users.

## 🚀 Features

* **User Authentication & Authorization**: Secure JWT-based authentication with Refresh Tokens. Includes reliable features like User Registration, Login, Email Confirmation, Forgot/Reset Password workflows, and Role-based access control (e.g., Admin vs User).
* **Event Management**: Full CRUD operations for creating and updating events, managing locations, dates, and handling custom banner image uploads.
* **Organizer Management**: Manage event organizers and their profiles, complete with custom logo uploads.
* **Ticket Management**: Create and manage different types of tickets using structured enums (e.g., VIP, Standard, Regular), managing both pricing and available quantities.
* **Comprehensive Testing**: Extensive fully functioning unit testing suite built with **xUnit**, **Moq**, and **FluentAssertions**, covering services and controllers to ensure reliable and clean code quality.

## 🛠️ Technology Stack

* **.NET 10** / ASP.NET Core Web API
* **Entity Framework Core** (EF Core) for database management and querying
* **JSON Web Tokens (JWT)** for secure, stateless API authentication
* **AutoMapper** for mapping internal domain Entities to flexible Data Transfer Objects (DTOs)
* **xUnit**, **Moq**, and **FluentAssertions** for Unit Testing

## 📁 Project Structure

* **`EventHub.Api/`** - Core API application containing application logic, DTOs, Entities, Controllers, Database configs, and Services.
* **`EventHub.Api.Tests/`** - Robust xUnit testing project containing fixtures, mocked data, and unit tests for controllers and services.

## 🚦 Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* An IDE such as JetBrains Rider, Visual Studio, or Visual Studio Code
* SQL Server or configured local database

### Running the API
1. Clone the repository to your local machine.
2. Navigate to the `EventHub.Api` folder.
3. Update the `appsettings.json` or `appsettings.Development.json` with your database connection strings, JWT settings, and SMTP (email) credentials.
4. Run database migrations to provision the schema:
   ```bash
   dotnet ef database update
   ```
5. Start the application:
   ```bash
   dotnet run
   ```

### Running Tests
To verify everything is working perfectly, navigate to the root directory containing the solution file and run the following command to execute the test suite:
```bash
dotnet test
```
