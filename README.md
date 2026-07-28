# MiWebCafe API

RESTful Web API designed to manage the daily operations of a cafeteria system, including authentication, sales management, and administrative reporting.

This backend provides secure endpoints consumed by multiple frontend applications (Angular and Next.js).

---

## Features

- JWT Authentication
- Role-based authorization (Admin / Cashier)
- Sales registration and management
- Cash register management (Open / Close)
- Secure RESTful endpoints
- DTO-based data transfer
- Entity Framework Core integration

---

## Tech Stack

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- RESTful API architecture

---

## Architecture Overview

The project follows clean separation of concerns:

- Controllers → Handle HTTP requests
- DTOs → Define data contracts
- Entities → Domain models
- Data → DbContext and configuration
- Migrations → Database version control

---

## Authentication & Security

The system implements:

- JWT token authentication
- Role-based authorization
- Middleware validation
- Secure endpoint protection using `[Authorize]`

Roles:
- Admin
- Cashier

---

## Local secrets

The API uses .NET Secret Manager in the `Development` environment. Secrets are stored in the developer profile and are not committed to the repository. The project already defines its `UserSecretsId`, so no initialization command is required.

Before starting the API for the first time, open PowerShell in `MiWebCafe.API` and configure all required values:

```powershell
$jwtKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))

dotnet user-secrets set "Jwt:Key" $jwtKey

dotnet user-secrets set "InitialUsers:Admin:Nombre" "<admin-name>"
dotnet user-secrets set "InitialUsers:Admin:Email" "<admin-email>"
dotnet user-secrets set "InitialUsers:Admin:Password" "<admin-password>"

dotnet user-secrets set "InitialUsers:Cajero:Nombre" "<cashier-name>"
dotnet user-secrets set "InitialUsers:Cajero:Email" "<cashier-email>"
dotnet user-secrets set "InitialUsers:Cajero:Password" "<cashier-password>"
```

Replace every placeholder with local values that are not committed or shared through the repository.

- `Jwt:Key` signs and validates JWTs. It must contain at least 32 characters; the command generates a cryptographically random 32-byte value.
- `InitialUsers:Admin:*` defines the administrator created only when its email does not already exist.
- `InitialUsers:Cajero:*` defines the cashier created only when its email does not already exist.

The application validates these values at startup. Do not add them to `appsettings.json`, `appsettings.Development.json`, or any tracked file.

---

## Database

- SQL Server
- Managed via Entity Framework Core Migrations

To update database:
---

## Running the Project Locally

1. Open solution in Visual Studio
2. Configure connection string in:
3. Run database migration
4. Start the API

Default URL: https://localhost:5001
---

## Frontend Clients

This API is consumed by:

- Angular Frontend
- Next.js Frontend

---

## Future Improvements

- Cloud deployment (Azure)
- API versioning
- Logging & monitoring
- Unit testing

---

## Author

Oscar Chinchin
