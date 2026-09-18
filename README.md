# 🇳🇿 NZWalks API & Web Platform

[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?logo=.net&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://learn.microsoft.com/ef/core/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture%20%7C%20CQRS-brightgreen)](#-architecture--navigation)
[![Tests](https://img.shields.io/badge/Tests-xUnit%20%7C%20FluentAssertions%20%7C%20Moq-blue)](tests/)

**NZWalks** is an enterprise-grade RESTful API and Web application built on **.NET 10** that enables tracking, exploring, and managing scenic walking tracks ("Walks") and geographical regions across New Zealand.

The project is built adhering to **Clean Architecture** principles and the **CQRS (Command Query Responsibility Segregation)** pattern via **MediatR**, featuring JWT authentication with role-based access control, dual database contexts, transactional email verification, image file handling, and automated test coverage.

---

## 📑 Table of Contents

- [Overview & Architecture](#-overview--architecture)
- [Project Structure & Navigation](#-project-structure--navigation)
- [Key Features](#-key-features)
- [API Reference for Consumers & Frontend](#-api-reference-for-consumers--frontend)
  - [Response Envelope Format (`Result`)](#1-response-envelope-format-result)
  - [Pagination Envelope (`PagedResponse<T>`)](#2-pagination-envelope-pagedresponset)
  - [Authentication & JWT Headers](#3-authentication--jwt-headers)
  - [Enums](#4-enums)
  - [API Endpoints Overview](#5-api-endpoints-overview)
  - [Detailed Endpoint Documentation](#6-detailed-endpoint-documentation)
- [Getting Started & Local Setup](#-getting-started--local-setup)
  - [Prerequisites](#prerequisites)
  - [Configuration](#configuration)
  - [Database Migrations](#database-migrations)
  - [Running the Applications](#running-the-applications)
  - [Running Unit & Integration Tests](#running-unit--integration-tests)
- [Contributing Guide](#-contributing-guide)

---

## 🏛 Overview & Architecture

NZWalks is structured using **Clean Architecture** (Onion/Hexagonal) to enforce separation of concerns, high maintainability, and testability.

```
                      ┌─────────────────────────────┐
                      │        NZWalks.UI           │ (ASP.NET Core MVC Client)
                      └──────────────┬──────────────┘
                                     │ HTTP
                                     ▼
                      ┌─────────────────────────────┐
                      │        NZWalks.APIs         │ (Presentation Layer / REST API)
                      └──────────────┬──────────────┘
                                     │
                                     ▼
                      ┌─────────────────────────────┐
                      │     NZWalks.Application     │ (CQRS Commands, Queries, DTOs, Handlers)
                      └──────────────┬──────────────┘
                                     ▲
                     ┌───────────────┴──────────────┐
                     │                              │
      ┌─────────────────────────────┐┌─────────────────────────────┐
      │     NZWalks.Infrastructure  ││       NZWalks.Domain        │
      │ (EF Core, Auth, SMTP, Repos)││ (Core Entities & Enums)     │
      └─────────────────────────────┘└─────────────────────────────┘
```

- **Domain Layer (`NZWalks.Domain`)**: Core enterprise business models (`Walk`, `Region`, `Image`) and domain enums (`DifficultyType`). It has zero external dependencies.
- **Application Layer (`NZWalks.Application`)**: Orchestrates business rules through CQRS commands and queries using **MediatR**, AutoMapper mappings, repository interfaces, custom validation, and response envelopes.
- **Infrastructure Layer (`NZWalks.Infrastructure`)**: Implements persistence via Entity Framework Core (`NZWalksDbContext` for domain data and `NZWalksAuthDbContext` for ASP.NET Identity), local physical image storage, JWT generation, and SMTP email services.
- **API Layer (`NZWalks.APIs`)**: Presentation layer providing HTTP endpoints, Swagger / OpenAPI documentation, CORS policies, Serilog logging, custom action filters, and static file serving.
- **UI Layer (`NZWalks.UI`)**: An ASP.NET Core MVC consumer that interacts with the API endpoints.

---

## 📂 Project Structure & Navigation

```text
NZWalks/
├── src/
│   ├── NZWalks.Domain/                  # Core domain layer
│   │   ├── Common/                      # Base entities (BaseEntity, NamedEntity)
│   │   ├── Entities/                    # Domain models (Walk, Region, Image)
│   │   └── Enums/                       # Domain enums (DifficultyType)
│   │
│   ├── NZWalks.Application/             # Application & business logic
│   │   ├── Common/                      # Unified wrappers (Result, PagedResponse, PaginationValidator)
│   │   ├── DTOs/                        # Request / Response transfer objects
│   │   ├── Extensions/                  # IQueryable pagination helpers
│   │   ├── Interfaces/                  # Contracts for Repositories and Services
│   │   ├── Mappings/                    # AutoMapper profiles
│   │   ├── Regions/                     # Region CQRS Commands & Queries (MediatR)
│   │   └── Walks/                       # Walk CQRS Commands & Queries (MediatR)
│   │
│   ├── NZWalks.Infrastructure/          # Data access & external services
│   │   ├── Data/                        # EF Core DbContexts (NZWalksDbContext, NZWalksAuthDbContext)
│   │   ├── Migrations/                  # EF Core database migrations
│   │   ├── Repositories/                # SQL repositories & local file storage
│   │   └── Services/                    # Auth, CurrentUser (Claims), and Email services
│   │
│   ├── NZWalks.APIs/                    # REST API presentation layer
│   │   ├── Controllers/                 # API Controllers (Auth, Regions, Walks, Images)
│   │   ├── CustomActionFilters/         # Custom attributes for model and file validation
│   │   ├── Images/                      # Uploaded image directory (served statically)
│   │   ├── appsettings.json             # Configuration & connection strings
│   │   └── Program.cs                   # Dependency injection, middleware pipeline, & Swagger
│   │
│   └── NZWalks.UI/                      # ASP.NET Core MVC consuming client
│       ├── Controllers/                 # MVC Controllers
│       ├── Views/                       # Razor Views
│       └── wwwroot/                     # Static assets (Bootstrap, CSS, JS)
│
└── tests/
    ├── NZWalks.API.Tests/               # Controller & Action filter unit tests
    ├── NZWalks.Application.Tests/       # Command/Query handlers, mapper & validator tests
    └── NZWalks.Infrastructure.Tests/    # Repository and Service unit tests
```

---

## 🚀 Key Features

- **Clean Architecture & CQRS**: Strict segregation of reads and writes via MediatR queries and commands.
- **Dual Database Architecture**: Separate databases/contexts for domain entities (`NZWalksDb`) and ASP.NET Core Identity (`NZWalksAuthDB`).
- **Secure Authentication & RBAC**:
  - JWT Bearer authentication with token validation.
  - Role-based authorization (`Reader`, `Writer`, `Admin`).
  - Strict role safeguards preventing self-assignment of `Admin`.
- **Transactional Email Verification & Security**:
  - Account confirmation via email link on registration.
  - Resend verification link capability.
  - Registration rolls back cleanly if the verification email fails to deliver.
  - Login email alert notifications.
- **Rich Walks & Region Management**:
  - Comprehensive CRUD operations.
  - Dynamic filtering by name (`filterOn=Name&filterQuery=...`).
  - Filtering walks by difficulty (`Easy`, `Moderate`, `Hard`) or region.
  - Personal walk tracking (`/api/Walks/user`) and personal longest walk calculation (`/api/Walks/user/longest`).
  - **Ownership checks**: Users can only modify/delete their own walks unless holding the `Admin` role.
- **Image File Uploads**:
  - Multi-part form file upload with custom validation (supports `.jpg`, `.jpeg`, `.png`, max 10MB).
  - Physical disk storage with dynamic URL generation and static file serving (`/Images/{filename}`).
- **Standardized Unified Response Envelope**:
  - Predictable API responses for clients via `Result` and `PagedResponse<T>`.
  - Built-in pagination metadata (`PageNumber`, `PageSize`, `TotalPages`, `TotalRecords`, `HasNextPage`, `HasPreviousPage`).
- **Comprehensive Testing Suite**: 122 automated unit and integration tests passing across API, Application, and Infrastructure layers using **xUnit**, **Moq**, and **FluentAssertions**.

---

## 📡 API Reference for Consumers & Frontend

### 1. Response Envelope Format (`Result`)

All API endpoints return responses encapsulated in a uniform `Result` JSON envelope:

```json
{
  "isSuccess": true,
  "status": 200,
  "message": "Operation completed successfully.",
  "data": { ... }
}
```

When an error occurs:

```json
{
  "isSuccess": false,
  "status": 400,
  "message": "Detailed error message describing what went wrong.",
  "data": null
}
```

### 2. Pagination Envelope (`PagedResponse<T>`)

Endpoints that return lists return data wrapped inside `PagedResponse<T>` within the `data` property:

```json
{
  "isSuccess": true,
  "status": 200,
  "message": "Walks retrieved successfully",
  "data": {
    "data": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "name": "Milford Track",
        "description": "One of New Zealand's finest walks.",
        "lengthInKm": 53.5,
        "walkImageUrl": "https://example.com/image.jpg",
        "difficultyType": "Moderate",
        "regionId": "906cb139-415a-4bbb-a174-1a1faf9fb1f6",
        "region": {
          "id": "906cb139-415a-4bbb-a174-1a1faf9fb1f6",
          "code": "STL",
          "name": "Southland",
          "regionImageUrl": "https://example.com/southland.jpg"
        }
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalRecords": 48,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### 3. Authentication & JWT Headers

For protected endpoints, include the JWT token returned from `/api/Auth/Login` in the `Authorization` header:

```http
Authorization: Bearer <your_jwt_token_here>
```

### 4. Enums

#### `DifficultyType`
Serialized as strings in JSON:
- `"Easy"`
- `"Moderate"`
- `"Hard"`

---

### 5. API Endpoints Overview

| Method | Endpoint | Description | Roles Required |
| :--- | :--- | :--- | :--- |
| **Authentication** | | | |
| `POST` | `/api/Auth/Register` | Register a new user account | Public |
| `GET` | `/api/Auth/VerifyEmail` | Verify email address from email link | Public |
| `POST` | `/api/Auth/Login` | Authenticate user & receive JWT token | Public |
| `POST` | `/api/Auth/ResendVerificationEmail` | Resend verification email link | Public |
| **Regions** | | | |
| `GET` | `/api/Regions` | Get paginated list of all regions | `Reader`, `Writer`, `Admin` |
| `GET` | `/api/Regions/{id}` | Get single region by its GUID | `Reader`, `Writer`, `Admin` |
| `POST` | `/api/Regions` | Create a new region | `Admin` |
| `PUT` | `/api/Regions/{id}` | Update an existing region | `Admin` |
| `DELETE`| `/api/Regions/{id}` | Delete a region | `Admin` |
| **Walks** | | | |
| `GET` | `/api/Walks` | Get all walks (supports filtering & pagination) | `Reader`, `Writer`, `Admin` |
| `GET` | `/api/Walks/{id}` | Get single walk by its GUID | `Reader`, `Writer`, `Admin` |
| `GET` | `/api/Walks/user` | Get walks created by the authenticated user | `Writer`, `Admin` |
| `GET` | `/api/Walks/user/longest` | Get longest walk created by the current user | `Writer`, `Admin` |
| `GET` | `/api/Walks/region/{regionId}` | Get walks within a specific region | `Reader`, `Writer`, `Admin` |
| `GET` | `/api/Walks/difficulty` | Get walks filtered by difficulty level | `Reader`, `Writer`, `Admin` |
| `POST` | `/api/Walks` | Create a new walk | `Writer`, `Admin` |
| `PUT` | `/api/Walks/{id}` | Update a walk (Owner only) | `Writer`, `Admin` |
| `DELETE`| `/api/Walks/{id}` | Delete a walk (Owner or Admin) | `Writer`, `Admin` |
| **Images** | | | |
| `POST` | `/api/Images/Upload` | Upload an image file (multipart/form-data) | Public / Authorized |

---

### 6. Detailed Endpoint Documentation

<details>
<summary><b>🔐 Authentication Endpoints</b></summary>

#### Register User
- **Method / Route**: `POST /api/Auth/Register`
- **Request Body**:
```json
{
  "username": "jane.doe@example.com",
  "password": "Password123!",
  "roles": ["Reader", "Writer"]
}
```
> **Note**: `username` must be a valid email. Allowed roles for self-registration: `Reader`, `Writer`. Self-assigning `Admin` is rejected.

- **Success Response (`201 Created`)**:
```json
{
  "isSuccess": true,
  "status": 201,
  "message": "Registration successful! Please check your email to verify your account.",
  "data": null
}
```

---

#### Verify Email
- **Method / Route**: `GET /api/Auth/VerifyEmail?userId={userId}&token={token}`
- **Success Response (`200 OK`)**:
```json
{
  "isSuccess": true,
  "status": 200,
  "message": "Email verified successfully! You can now log in.",
  "data": null
}
```

---

#### Login
- **Method / Route**: `POST /api/Auth/Login`
- **Request Body**:
```json
{
  "username": "jane.doe@example.com",
  "password": "Password123!"
}
```
- **Success Response (`200 OK`)**:
```json
{
  "isSuccess": true,
  "status": 200,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
    "userId": "c56a4180-65aa-42ec-a945-5fd21dec0538",
    "roles": ["Reader", "Writer"]
  }
}
```

---

#### Resend Verification Email
- **Method / Route**: `POST /api/Auth/ResendVerificationEmail`
- **Request Body**:
```json
{
  "email": "jane.doe@example.com"
}
```
- **Success Response (`200 OK`)**:
```json
{
  "isSuccess": true,
  "status": 200,
  "message": "A new verification email has been sent. Please check your inbox.",
  "data": null
}
```
</details>

<details>
<summary><b>📍 Region Endpoints</b></summary>

#### Get All Regions
- **Method / Route**: `GET /api/Regions?pageNumber=1&pageSize=10`
- **Auth**: `Reader`, `Writer`, or `Admin`
- **Success Response (`200 OK`)**:
```json
{
  "isSuccess": true,
  "status": 200,
  "message": "Regions retrieved successfully",
  "data": {
    "data": [
      {
        "id": "f7248fc3-2585-4efb-8d1d-1c555f4087f6",
        "code": "AKL",
        "name": "Auckland",
        "regionImageUrl": "https://example.com/auckland.jpg"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "totalRecords": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

---

#### Get Region By ID
- **Method / Route**: `GET /api/Regions/{id}`
- **Auth**: `Reader`, `Writer`, or `Admin`

---

#### Create Region
- **Method / Route**: `POST /api/Regions`
- **Auth**: `Admin` only
- **Request Body**:
```json
{
  "code": "WGN",
  "name": "Wellington",
  "regionImageUrl": "https://example.com/wellington.jpg"
}
```

---

#### Update Region
- **Method / Route**: `PUT /api/Regions/{id}`
- **Auth**: `Admin` only
- **Request Body**:
```json
{
  "code": "WGN",
  "name": "Wellington Region",
  "regionImageUrl": "https://example.com/wellington-updated.jpg"
}
```

---

#### Delete Region
- **Method / Route**: `DELETE /api/Regions/{id}`
- **Auth**: `Admin` only
</details>

<details>
<summary><b>🥾 Walk Endpoints</b></summary>

#### Get All Walks
- **Method / Route**: `GET /api/Walks?filterOn=Name&filterQuery=Track&pageNumber=1&pageSize=10`
- **Query Parameters**:
  - `filterOn` (optional): Field to filter by (e.g. `Name`).
  - `filterQuery` (optional): Filter keyword.
  - `pageNumber` (optional, default `1`): Page number to retrieve.
  - `pageSize` (optional, default `10`, max `50`): Number of items per page.
- **Auth**: `Reader`, `Writer`, or `Admin`

---

#### Get Walk By ID
- **Method / Route**: `GET /api/Walks/{id}`
- **Auth**: `Reader`, `Writer`, or `Admin`

---

#### Get Walks By Region
- **Method / Route**: `GET /api/Walks/region/{regionId}?pageNumber=1&pageSize=10`
- **Auth**: `Reader`, `Writer`, or `Admin`

---

#### Get Walks By Difficulty
- **Method / Route**: `GET /api/Walks/difficulty?difficulty=Moderate&pageNumber=1&pageSize=10`
- **Auth**: `Reader`, `Writer`, or `Admin`
- **Query Parameters**: `difficulty` values: `Easy`, `Moderate`, `Hard`.

---

#### Get User's Walks
- **Method / Route**: `GET /api/Walks/user?pageNumber=1&pageSize=10`
- **Auth**: `Writer` or `Admin` (Retrieves walks created by the current token's user).

---

#### Get User's Longest Walk
- **Method / Route**: `GET /api/Walks/user/longest`
- **Auth**: `Writer` or `Admin`

---

#### Create Walk
- **Method / Route**: `POST /api/Walks`
- **Auth**: `Writer` or `Admin`
- **Request Body**:
```json
{
  "name": "Tongariro Alpine Crossing",
  "description": "A world-renowned alpine trek across active volcanic terrain.",
  "lengthInKm": 19.4,
  "walkImageUrl": "https://example.com/tongariro.jpg",
  "difficultyType": "Hard",
  "regionId": "cfa06ed2-bf65-4b65-93ed-c9d286ddb0de"
}
```

---

#### Update Walk
- **Method / Route**: `PUT /api/Walks/{id}`
- **Auth**: `Writer` or `Admin` (*Note: Only the user who created the walk can update it*).
- **Request Body**: Same schema as Create Walk.

---

#### Delete Walk
- **Method / Route**: `DELETE /api/Walks/{id}`
- **Auth**: `Writer` or `Admin` (*Note: Only the owner or an Admin can delete the walk*).
</details>

<details>
<summary><b>🖼 Image Upload Endpoints</b></summary>

#### Upload Image
- **Method / Route**: `POST /api/Images/Upload`
- **Content-Type**: `multipart/form-data`
- **Form Fields**:
  - `File`: File binary (allowed extensions: `.jpg`, `.jpeg`, `.png`, max file size: `10MB`).
  - `FileName`: Name identifier for the image.
  - `FileDescription` (optional): Brief description.
- **Success Response (`200 OK`)**:
```json
{
  "id": "95a5fbf8-53b9-4113-91c2-641e1765c92d",
  "fileName": "TongariroCrossing",
  "fileDescription": "Scenic view from the summit",
  "fileExtension": ".jpg",
  "fileSizeInBytes": 2048500,
  "filePath": "https://localhost:7246/Images/TongariroCrossing.jpg"
}
```
</details>

---

## 🛠 Getting Started & Local Setup

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or higher.
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express / LocalDB).
- An IDE such as [Visual Studio 2022 / 2025](https://visualstudio.microsoft.com/), [VS Code](https://code.visualstudio.com/), or [JetBrains Rider](https://www.jetbrains.com/rider/).

---

### Configuration

Update `src/NZWalks.APIs/appsettings.json` (or use .NET `user-secrets`) with your database connections and SMTP credentials:

```json
{
  "ConnectionStrings": {
    "NZWalksConnectionString": "Server=localhost;Database=NZWalksDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "NZWalksAuthConnectionString": "Server=localhost;Database=NZWalksAuthDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyWithAtLeast32CharactersLong!",
    "Issuer": "https://localhost:7246",
    "Audience": "https://localhost:7246"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "NZ Walks Support",
    "Password": "your-app-password",
    "BaseUrl": "https://localhost:7246"
  }
}
```

---

### Database Migrations

Apply the Entity Framework Core migrations to create and seed the databases:

```bash
# 1. Update Domain Database (NZWalksDb)
dotnet ef database update --project src/NZWalks.Infrastructure --startup-project src/NZWalks.APIs --context NZWalksDbContext

# 2. Update Auth & Identity Database (NZWalksAuthDB)
dotnet ef database update --project src/NZWalks.Infrastructure --startup-project src/NZWalks.APIs --context NZWalksAuthDbContext
```

---

### Running the Applications

#### Run the Web API
```bash
dotnet run --project src/NZWalks.APIs
```
- Swagger UI will be available at: `https://localhost:7246/swagger` (or matching port).

#### Run the MVC Frontend Client
```bash
dotnet run --project src/NZWalks.UI
```

---

### Running Unit & Integration Tests

Run the full automated test suite covering all layers:

```bash
dotnet test
```

---

## 🤝 Contributing Guide

1. **Fork & Branch**: Create a new feature branch (`git checkout -b feature/your-feature-name`).
2. **Follow Layered Architecture**:
   - Entities & Enums belong in `NZWalks.Domain`.
   - Business use-cases, CQRS Commands/Queries, and DTOs belong in `NZWalks.Application`.
   - Data persistence, database migrations, and 3rd-party services belong in `NZWalks.Infrastructure`.
   - Controllers, routes, and filters belong in `NZWalks.APIs`.
3. **Add Tests**: Write unit tests for all new command/query handlers and controllers in `tests/`.
4. **Commit & Pull Request**: Ensure `dotnet test` passes with zero failures before submitting your PR.
