# Dealership API

A backend learning project built with **ASP.NET Core 10 (Minimal APIs)**, **Entity Framework Core**, and **PostgreSQL**, running in Docker. This project is a hands-on exploration of building a RESTful API in C#/.NET, coming from a background in Node.js/Express and TypeScript.

The domain: a car dealership platform where dealerships employ users and sell vehicles. Built to practice backend fundamentals — routing, payload handling, relational data modeling, an ORM, and JWT-based authentication/authorization.

## Tech Stack

- **.NET 10** — Minimal API style
- **Entity Framework Core** — ORM, using the `Npgsql.EntityFrameworkCore.PostgreSQL` provider
- **PostgreSQL 17** (Alpine) — running in Docker
- **Docker Compose** — for local database orchestration
- **JWT (JSON Web Tokens)** — planned for authentication/authorization
- **BCrypt** — planned for password hashing

## Project Status

🚧 Work in progress — actively being built as a learning exercise.

- [x] Dockerized PostgreSQL database
- [x] .NET 10 Web API project scaffolded
- [x] Git repository initialized and pushed to GitHub
- [x] Data models defined (`User`, `Dealership`, `Vehicle` → `Automobile`)
- [x] EF Core `DbContext` configured with table-per-hierarchy inheritance
- [x] Initial migration generated and applied to the database
- [ ] Database seed script (with hashed passwords)
- [ ] JWT authentication (login endpoint issuing tokens)
- [ ] Authorization (role-based and dealership-scoped access control)
- [ ] Full CRUD endpoints for Dealerships, Users, and Vehicles
- [ ] End-to-end testing via Postman

## Data Model

Three related entities, connected via foreign keys:

```
Dealership (1) ──< (many) User
Dealership (1) ──< (many) Vehicle
```

- **Dealership** — a physical dealership location (name, address, budget, contact info, etc.)
- **User** — an employee or stakeholder belonging to a dealership (nullable dealership link for global/admin accounts), with a role and a hashed password
- **Vehicle** — an abstract base entity representing anything sellable. Currently has one concrete subtype, **Automobile** (cars), with the model designed to support additional vehicle types (motorbikes, boats, planes) in the future without restructuring.

### Inheritance strategy

`Vehicle` and its subtypes use EF Core's **Table-Per-Hierarchy (TPH)** mapping: all vehicle types share a single `Vehicles` table, distinguished by a `VehicleType` discriminator column that EF Core manages automatically. This was a deliberate choice to practice EF Core's inheritance mapping and keep the schema extensible as new vehicle types are added.

## Local Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (with Docker Compose)
- `dotnet-ef` CLI tool:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 1. Start the database

From the repository root:

```bash
docker compose up -d
```

This spins up a PostgreSQL 17 container with a named volume for data persistence. Configuration lives in `docker-compose.yml`.

### 2. Configure the connection string

This project uses [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to keep local credentials out of source control. From inside the `DealershipApi/` project folder:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=postgres;Username=admin;Password=admin"
```

(Adjust the values to match your local `docker-compose.yml` credentials if they differ.)

### 3. Apply database migrations

```bash
cd DealershipApi
dotnet ef database update
```

This creates the `Users`, `Dealerships`, and `Vehicles` tables (plus EF Core's internal migrations history table) in the running Postgres container.

### 4. Run the API

```bash
dotnet run
```

The console output will show the local URL(s) the API is listening on.

## Project Structure

```
DealershipApp/
├── docker-compose.yml         # PostgreSQL service definition
├── .gitignore
├── README.md
└── DealershipApi/
    ├── Program.cs              # App entry point, endpoint definitions, middleware pipeline
    ├── Models/
    │   ├── Vehicle.cs          # Abstract base entity + Automobile subtype
    │   ├── User.cs
    │   └── Dealership.cs
    ├── Data/
    │   └── AppDbContext.cs     # EF Core DbContext, DbSets, inheritance config
    └── Migrations/             # EF Core migration history (committed to source control)
```

## Notes on Design Decisions

- **Minimal APIs over Controllers**: chosen initially for its closer resemblance to Express.js route handlers, making the transition from a Node.js background more intuitive while learning ASP.NET Core fundamentals.
- **User Secrets over `.env`**: .NET doesn't use `.env` files natively; User Secrets is the framework-native equivalent for local development, storing sensitive config outside the project directory entirely so it can never accidentally be committed.
- **Migrations are committed to git**: unlike build artifacts (`bin/`, `obj/`), EF Core migrations are source code describing schema history and are required for anyone cloning the repo to set up a working database.

## License

This is a personal learning/portfolio project. Feel free to browse or fork for your own learning purposes.