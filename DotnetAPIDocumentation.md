# DotnetAPI

A RESTful Web API built with **ASP.NET Core (.NET 10)**, **Entity Framework Core**, and **MySQL**. It features JWT authentication, rate limiting, Scalar API documentation, and article/content management.

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack & Prerequisites](#-tech-stack--prerequisites)
- [Configuration](#-configuration)
- [Database Setup & Migrations](#-database-setup--migrations)
- [Running the Project Locally](#-running-the-project-locally)
- [Running with Docker](#-running-with-docker)
- [API Documentation](#-api-documentation)
- [Running Tests](#-running-tests)
- [Project Structure](#-project-structure)

---

## ✨ Features

- **Authentication & Authorization**: User registration and login using JWT tokens and password hashing.
- **Article & Content Management**: CRUD operations for Articles, Categories, Tags, and Comments.
- **Rate Limiting**: Fixed-window rate limiting configured for general API traffic and authentication routes.
- **Interactive Documentation**: Interactive API testing and exploration via **Scalar** (`/scalar/v1`) and OpenAPI v1 specifications.
- **CORS Support**: Configured for modern frontend dev servers (Vite, Next.js / React).

---

## 🛠 Tech Stack & Prerequisites

### Technologies
- **Framework**: .NET 10.0 (C# 14)
- **Database**: MySQL 8.0+ / MariaDB
- **ORM**: Entity Framework Core 9 with Pomelo MySQL Provider
- **Documentation**: Scalar ASP.NET Core & Microsoft OpenAPI
- **Testing**: xUnit & EF Core InMemory Database
- **Containerization**: Docker & Docker Compose

### Prerequisites
Before getting started, make sure you have the following installed:
1. [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
2. [MySQL Server](https://dev.mysql.com/downloads/) (or run MySQL via Docker)
3. [EF Core CLI Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (optional, for migrations):
   ```bash
   dotnet tool install --global dotnet-ef
   ```
4. [Docker & Docker Compose](https://www.docker.com/) *(optional, for containerized deployment)*

---

## ⚙️ Configuration

Application settings are located in `DotnetAPI/appsettings.json` (or `DotnetAPI/appsettings.Development.json`).

### Key Settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=shopsphere;user=root;password=your_password;"
  },
  "ApiSettings": {
    "Secret": "YourStrongSecretKeyAtLeast32BytesLong!",
    "Issuer": "localhost:5023",
    "Audience": "localhost:5023",
    "AccessTokenExpiryMinutes": 60
  },
  "Frontend": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://localhost:3000"
    ]
  }
}
```
