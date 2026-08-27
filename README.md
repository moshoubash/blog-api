# 🚀 DotnetAPI - Blog & Article Platform

A robust, production-grade RESTful Web API built with **ASP.NET Core (.NET 10)**, **Entity Framework Core 9**, and **MySQL**. It features JWT Bearer authentication, role-based authorization, granular rate limiting, interactive API documentation powered by **Scalar**, automated slug generation, and a complete suite of content management endpoints.

---

## 📋 Table of Contents

- [✨ Features](#-features)
- [🛠 Tech Stack & Prerequisites](#-tech-stack--prerequisites)
- [⚙️ Configuration](#️-configuration)
- [🗄 Database Setup & Migrations](#-database-setup--migrations)
- [🚀 Running the Project Locally](#-running-the-project-locally)
- [🐳 Running with Docker](#-running-with-docker)
- [📖 API Documentation (Scalar & OpenAPI)](#-api-documentation-scalar--openapi)
- [🔌 API Endpoints Reference](#-api-endpoints-reference)
- [🧪 Running Tests](#-running-tests)
- [📂 Project Structure](#-project-structure)

---

## ✨ Features

- **JWT Authentication & Role-Based Authorization**: Secure user registration and login with password hashing via ASP.NET Identity `IPasswordHasher` and role claims (`Author`, `Admin`).
- **Article & Content Management**: Full CRUD operations for Articles, Categories, Tags, and Comments with pagination and search/filter support.
- **Automated Slug Generation**: Dynamic URL-friendly slug generation and retrieval for SEO-friendly URLs.
- **Repository Pattern**: Clean separation of database persistence using `IArticleRepository` and `ArticleRepository`.
- **Rate Limiting**: Built-in fixed-window rate limiters:
  - `api`: 60 requests per minute.
  - `login`: 5 requests per minute for brute-force protection.
- **Interactive Documentation**: Beautiful, modern API playground and exploration powered by **Scalar** (`/scalar/v1`) and OpenAPI v1 (`/openapi/v1.json`).
- **CORS Support**: Pre-configured policy for modern frontend dev environments (e.g. Vite, React, Next.js).
- **Automated Unit Testing**: Comprehensive repository tests with EF Core In-Memory database and xUnit.
- **Container Ready**: Multi-stage Alpine-based `Dockerfile` and `compose.yaml` for containerized development and deployment.

---

## 🛠 Tech Stack & Prerequisites

### Technologies

- **Runtime & Framework**: [.NET 10.0](https://dotnet.microsoft.com/download) (C# 14) / ASP.NET Core Web API
- **Database**: MySQL 8.0+ / MariaDB
- **ORM**: Entity Framework Core 9.0 (`Pomelo.EntityFrameworkCore.MySql`)
- **Authentication**: JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) & Identity Core
- **Documentation**: [Scalar.AspNetCore](https://scalar.com) & Microsoft.AspNetCore.OpenApi
- **Testing**: xUnit & EF Core In-Memory Database
- **Containerization**: Docker & Docker Compose

### Prerequisites

Before getting started, make sure you have the following installed:

1. [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
2. [MySQL Server](https://dev.mysql.com/downloads/) (or run MySQL via Docker)
3. [EF Core CLI Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (recommended for running migrations):
   ```bash
   dotnet tool install --global dotnet-ef
   ```
4. [Docker & Docker Compose](https://www.docker.com/) _(optional, for containerized execution)_

---

## ⚙️ Configuration

Local application settings are stored in `appsettings.json` and `appsettings.Development.json`. These files are intentionally ignored by Git because they contain database credentials and signing secrets.

For a new checkout, copy the safe template and replace every placeholder with local values:

```bash
cp appsettings.example.json appsettings.json
```

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=shopsphere;user=root;password=your_password;"
  },
  "ApiSettings": {
    "Secret": "YourSuperSecretKeyMustBeAtLeast32CharactersLong!",
    "Issuer": "localhost:5023",
    "Audience": "localhost:5023",
    "AccessTokenExpiryMinutes": 60
  },
  "Frontend": {
    "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
  }
}
```

> [!IMPORTANT]
> Never commit real passwords, JWT secrets, API keys, or other sensitive values. Use a strong unique value for `ApiSettings:Secret` (at least 32 characters / 256 bits). Any credentials that were previously committed must be rotated, even after the files are removed from tracking.

---

## 🗄 Database Setup & Migrations

The project uses Entity Framework Core with code-first migrations.

### 1. Apply Migrations to Database

Apply all pending migrations to create and update your database schema:

```bash
dotnet ef database update
```

### 2. Adding New Migrations (Optional)

If you modify or add any entity models:

```bash
dotnet ef migrations add <MigrationName>
```

---

## 🚀 Running the Project Locally

### 1. Restore and Build

```bash
dotnet restore
dotnet build
```

### 2. Run the Application

```bash
dotnet run --launch-profile http
```

The API will start at:

- **Base URL**: `http://localhost:5023`
- **Scalar API UI**: `http://localhost:5023/scalar/v1`
- **OpenAPI Schema**: `http://localhost:5023/openapi/v1.json`

---

## 🐳 Running with Docker

You can easily build and run the API inside a Docker container:

### Using Docker Compose

```bash
docker compose up --build
```

The containerized service will be exposed at:

- **API URL**: `http://localhost:8080`
- **Scalar UI**: `http://localhost:8080/scalar/v1`

### Standalone Docker Build & Run

```bash
# Build the Docker image
docker build -t dotnet-api .

# Run the container
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development dotnet-api
```

---

## 📖 API Documentation (Scalar & OpenAPI)

When running in `Development` mode, interactive API documentation is enabled:

- **Scalar Interactive UI**: Navigate to [`http://localhost:5023/scalar/v1`](http://localhost:5023/scalar/v1) in your browser.
- **OpenAPI v1 Spec**: Access the JSON schema at [`http://localhost:5023/openapi/v1.json`](http://localhost:5023/openapi/v1.json).

### Authorizing in Scalar

1. Register/Login via `/api/auth/login` to obtain your JWT token.
2. Click the **Authorize / Bearer** button in the Scalar interface.
3. Paste the token without the `Bearer ` prefix (Scalar attaches the scheme automatically).

---

## 🔌 API Endpoints Reference

### 🔐 Authentication (`/api/auth`)

| Method | Endpoint             | Auth Required              | Description                                          |
| ------ | -------------------- | -------------------------- | ---------------------------------------------------- |
| `POST` | `/api/auth/register` | No                         | Register a new user (`name`, `username`, `password`) |
| `POST` | `/api/auth/login`    | No _(Rate Limited: 5/min)_ | Authenticate user and return JWT token               |

### 👤 Users (`/api/users`)

| Method | Endpoint                 | Auth Required | Description                                        |
| ------ | ------------------------ | ------------- | -------------------------------------------------- |
| `GET`  | `/api/users/me`          | Bearer        | Get the current authenticated user profile & roles |
| `GET`  | `/api/users/me/articles` | Bearer        | Get all articles written by the current user       |

### 📝 Articles (`/api/articles`)

| Method   | Endpoint                    | Auth Required           | Description                                                                        |
| -------- | --------------------------- | ----------------------- | ---------------------------------------------------------------------------------- |
| `GET`    | `/api/articles`             | No                      | List published articles (supports `page`, `pageSize`, `search`, `category`, `tag`) |
| `GET`    | `/api/articles/{id}`        | No                      | Get single article by ID                                                           |
| `GET`    | `/api/articles/slug/{slug}` | No                      | Get single article by Slug                                                         |
| `POST`   | `/api/articles`             | Bearer (Author)         | Create a new article                                                               |
| `PUT`    | `/api/articles/{id}`        | Bearer (Author & Owner) | Update an existing article                                                         |
| `DELETE` | `/api/articles/{id}`        | Bearer (Author & Owner) | Delete an article                                                                  |

### 🏷 Categories (`/api/categories`)

| Method | Endpoint          | Auth Required   | Description           |
| ------ | ----------------- | --------------- | --------------------- |
| `GET`  | `/api/categories` | No              | Get all categories    |
| `POST` | `/api/categories` | Bearer (Author) | Create a new category |

### 📌 Tags (`/api/tags`)

| Method | Endpoint    | Auth Required   | Description      |
| ------ | ----------- | --------------- | ---------------- |
| `GET`  | `/api/tags` | No              | Get all tags     |
| `POST` | `/api/tags` | Bearer (Author) | Create a new tag |

### 💬 Comments (`/api/articles/{articleId}/comments`)

| Method   | Endpoint                                  | Auth Required  | Description                              |
| -------- | ----------------------------------------- | -------------- | ---------------------------------------- |
| `GET`    | `/api/articles/{articleId}/comments`      | No             | Get all approved comments for an article |
| `POST`   | `/api/articles/{articleId}/comments`      | Bearer         | Post a new comment                       |
| `DELETE` | `/api/articles/{articleId}/comments/{id}` | Bearer (Owner) | Delete a comment                         |

> [!TIP]
> You can also use the included [`DotnetAPI.http`](DotnetAPI.http) file in Rider or VS Code (with the REST Client extension) to test all endpoints interactively.

---

## 🧪 Running Tests

Unit tests are implemented with **xUnit** and **EF Core InMemory Database**:

```bash
dotnet test DotnetAPI.Tests/DotnetAPI.Tests.csproj
```

---

## 📂 Project Structure

```text
DotnetAPI/
├── Controllers/              # API Controllers (Auth, Articles, Categories, Tags, Comments, Users)
├── Database/                 # EF Core DbContext & model configurations
├── Dtos/                     # Data Transfer Objects (Article, Authentication, Blog, Common)
├── Migrations/               # EF Core database migrations
├── Models/                   # Entity domain models (Article, User, Role, Category, Tag, Comment)
├── Repositories/             # Data access abstractions & repository implementations
├── Services/                 # Business logic services (JwtTokenService, SlugService)
├── Properties/               # launchSettings.json configuration
├── DotnetAPI.Tests/          # Unit tests project (xUnit)
├── DotnetAPI.http            # HTTP request scratchpad for manual testing
├── Dockerfile                # Multi-stage Docker build file
├── compose.yaml              # Docker Compose service definition
├── Program.cs                # Application entry point & dependency injection setup
├── appsettings.json          # Main application configuration
└── README.md                 # Project documentation
```
