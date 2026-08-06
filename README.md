# Dealership API

A backend learning project built with **ASP.NET Core 10 (Minimal APIs)**, **Entity Framework Core**, and **PostgreSQL**, running in Docker. This project is a hands-on exploration of building a RESTful API in C#/.NET, coming from a background in Node.js/Express and TypeScript.

The domain: a car dealership platform where dealerships employ users and sell vehicles. Built to practice backend fundamentals — routing, payload handling, relational data modeling, an ORM, and JWT-based authentication/authorization.

## Tech Stack

- **.NET 10** — Minimal API style
- **Entity Framework Core** — ORM, using the `Npgsql.EntityFrameworkCore.PostgreSQL` provider
- **PostgreSQL 17** (Alpine) — running in Docker
- **Docker Compose** — for local database orchestration
- **JWT (JSON Web Tokens)** — stateless authentication via `Microsoft.AspNetCore.Authentication.JwtBearer`
- **BCrypt** (`BCrypt.Net-Next`) — password hashing

## Project Status

🚧 Work in progress — actively being built as a learning exercise.

- [x] Dockerized PostgreSQL database
- [x] .NET 10 Web API project scaffolded
- [x] Git repository initialized and pushed to GitHub
- [x] Data models defined (`User`, `Dealership`, `Vehicle` → `Automobile`)
- [x] EF Core `DbContext` configured with table-per-hierarchy inheritance
- [x] Initial migration generated and applied to the database
- [x] Database seed script (with BCrypt-hashed passwords)
- [x] JWT authentication — login endpoint issuing signed tokens
- [x] Route protection via `RequireAuthorization()`
- [ ] Role-based authorization (e.g. Admin-only endpoints)
- [ ] Dealership-scoped authorization (users restricted to their own dealership's data)
- [ ] Full CRUD endpoints for Dealerships, Users, and Vehicles
- [ ] Endpoint definitions split across multiple files (currently all in `Program.cs`)
- [ ] End-to-end testing via Postman collection
- [ ] Refresh tokens

## Data Model

Three related entities, connected via foreign keys:

```
Dealership (1) ──< (many) User
Dealership (1) ──< (many) Vehicle
```

- **Dealership** — a physical dealership location (name, address, budget, contact info, etc.)
- **User** — an employee or stakeholder belonging to a dealership (nullable dealership link for global/admin accounts), with a role and a BCrypt-hashed password
- **Vehicle** — an abstract base entity representing anything sellable. Currently has one concrete subtype, **Automobile** (cars), with the model designed to support additional vehicle types (motorbikes, boats, planes) in the future without restructuring.

### Inheritance strategy

`Vehicle` and its subtypes use EF Core's **Table-Per-Hierarchy (TPH)** mapping: all vehicle types share a single `Vehicles` table, distinguished by a `VehicleType` discriminator column that EF Core manages automatically. This was a deliberate choice to practice EF Core's inheritance mapping and keep the schema extensible as new vehicle types are added.

## Authentication

Authentication is stateless, via JWT:

1. `POST /api/login` accepts a username and password.
2. The server looks up the user in Postgres and verifies the password against the stored BCrypt hash — plaintext passwords are never stored or compared.
3. On success, a signed JWT is issued containing the user's name, role, and dealership ID as claims. Tokens are **not** persisted anywhere server-side — validation is done purely by checking the signature and expiry on each request, so there is no session store or "logged in users" table.
4. Protected endpoints are marked with `.RequireAuthorization()`. Requests without a valid `Authorization: Bearer <token>` header are rejected with `401 Unauthorized`.

The JWT signing key is stored via .NET User Secrets (`Jwt:Key`), not committed to source control.

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

### 2. Configure secrets

This project uses [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to keep local credentials out of source control. From inside the `DealershipApi/` project folder:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=postgres;Username=admin;Password=admin"
dotnet user-secrets set "Jwt:Key" "<a-long-random-secret-at-least-32-characters>"
```

(Adjust the connection string values to match your local `docker-compose.yml` credentials if they differ. The JWT key must be at least 256 bits / 32 characters for HMAC-SHA256 signing.)

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

The database is automatically seeded on first run (2 dealerships, 7 users, 21 vehicles) if the `Dealerships` table is empty — see `Data/DbInitializer.cs`.

The console output will show the local URL(s) the API is listening on.

### 5. Try it out

```bash
curl -X POST https://localhost:<port>/api/login \
  -H "Content-Type: application/json" \
  -d '{"username":"Jenny","password":"Jenny1967!"}' -k
```

Copy the returned token, then call a protected endpoint:

```bash
curl https://localhost:<port>/api/vehicles \
  -H "Authorization: Bearer <token>" -k
```

## Project Structure

```
DealershipApp/
├── docker-compose.yml         # PostgreSQL service definition
├── .gitignore
├── README.md
└── DealershipApi/
    ├── Program.cs              # App entry point, DI setup, auth config, endpoint definitions
    ├── Models/
    │   ├── Vehicle.cs          # Abstract base entity + Automobile subtype
    │   ├── User.cs
    │   └── Dealership.cs
    ├── Data/
    │   ├── AppDbContext.cs     # EF Core DbContext, DbSets, inheritance config
    │   └── DbInitializer.cs    # Seed data (dealerships, users, vehicles)
    └── Migrations/             # EF Core migration history (committed to source control)
```

## Notes on Design Decisions

- **Minimal APIs over Controllers**: chosen initially for its closer resemblance to Express.js route handlers, making the transition from a Node.js background more intuitive while learning ASP.NET Core fundamentals.
- **User Secrets over `.env`**: .NET doesn't use `.env` files natively; User Secrets is the framework-native equivalent for local development, storing sensitive config outside the project directory entirely so it can never accidentally be committed.
- **Migrations are committed to git**: unlike build artifacts (`bin/`, `obj/`), EF Core migrations are source code describing schema history and are required for anyone cloning the repo to set up a working database.
- **No server-side token storage**: JWTs are stateless by design — the tradeoff is that tokens can't be revoked before they expire without adding extra infrastructure (e.g. a token blocklist). Acceptable for this project's current scope; noted as a possible future improvement alongside refresh tokens.

## License

This is a personal learning/portfolio project. Feel free to browse or fork for your own learning purposes.