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
