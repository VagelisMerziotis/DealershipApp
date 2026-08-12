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
- [x] Role-based authorization (e.g. Admin-only endpoints)
- [x] Dealership-scoped authorization (users restricted to their own dealership's data)
- [x] Full CRUD endpoints for Dealerships
- [x] Cascade delete — removing a Dealership removes its Users and Vehicles
- [ ] Full CRUD endpoints for Users
- [ ] Full CRUD endpoints for Vehicles
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

### Cascade delete

`Vehicle.DealershipId` and `User.DealershipId` are both configured with `OnDelete(DeleteBehavior.Cascade)` in `AppDbContext.OnModelCreating`. Deleting a dealership deletes all of its vehicles and employee accounts along with it — a deliberate simplification for this project's scope. In a production system, this would more likely require reassignment or a soft-delete approach rather than an unconditional cascade.

## Authentication

Authentication is stateless, via JWT:

1. `POST /api/login` accepts a username and password.
2. The server looks up the user in Postgres and verifies the password against the stored BCrypt hash — plaintext passwords are never stored or compared.
3. On success, a signed JWT is issued containing the user's name, role, and dealership ID as claims. Tokens are **not** persisted anywhere server-side — validation is done purely by checking the signature and expiry on each request, so there is no session store or "logged in users" table.
4. Protected endpoints are marked with `.RequireAuthorization()`. Requests without a valid `Authorization: Bearer <token>` header are rejected with `401 Unauthorized`.

The JWT signing key is stored via .NET User Secrets (`Jwt:Key`), not committed to source control.

### Authorization model

Authorization combines two mechanisms:

- **Role-based** — endpoints restricted via `RequireAuthorization(policy => policy.RequireRole(...))`, e.g. only `Admin` can create or delete a dealership.
- **Dealership-scoped** — for resources like vehicles, non-Admin/Manager users only see data belonging to their own dealership (read from the `dealershipId` claim in their token), while `Admin` and `Manager` roles can access any dealership's data.

| Action | Endpoint | Who |
|---|---|---|
| List vehicles | `GET /api/vehicles` | Any authenticated user — Admin/Manager see all dealerships, others see only their own |
| Order a new vehicle | `POST /api/orderCar` | Admin only, scoped to their own dealership |
| Get dealership info | `GET /api/getDealershipInfo/{id}` | Admin or Manager, any dealership |
| List all dealerships | `GET /api/getDealerships` | Admin or Manager |
| Create dealership | `POST /api/createDealership` | Admin only |
| Update dealership | `PUT /api/dealerships/{id}` | Admin only |
| Delete dealership | `DELETE /api/deleteDealership/{id}` | Admin only |

## Dealership CRUD

Full CRUD is implemented for the `Dealership` entity:

- **Create** (`POST /api/createDealership`) — accepts a `CreateDealershipRequest` DTO (not the raw `Dealership` entity), so clients can't set server-controlled fields like `Id`, `Created`, or `Modified`. The server stamps timestamps and persists the new dealership.
- **Read** — `GET /api/getDealershipInfo/{id}` for a single dealership's summary (name, budget, address, employees), `GET /api/getDealerships` for the full list.
- **Update** (`PUT /api/dealerships/{id}`) — accepts a `ModifyDealershipRequest` DTO where every field is nullable, representing a partial update. Rather than a long chain of manual `if (request.X is not null) dealership.X = request.X;` checks, this endpoint uses **reflection** (`System.Reflection.PropertyInfo`) to loop over the DTO's properties, skip any that are `null` (not provided by the client), and copy the rest onto the matching property of the tracked `Dealership` entity by name. This was a deliberate exercise in C# reflection as an alternative to writing out each field explicitly — the trade-off is that renaming a property on one side without the other fails silently at runtime rather than at compile time, whereas explicit field-by-field checks would catch that immediately.
- **Delete** (`DELETE /api/deleteDealership/{id}`) — uses EF Core's `ExecuteDeleteAsync()` for a direct, single-round-trip database delete rather than loading the entity first. Cascades to the dealership's vehicles and users (see above).

Users and Vehicles CRUD follow the same DTO-in/entity-out pattern and are next in progress.

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
    │   ├── AppDbContext.cs     # EF Core DbContext, DbSets, inheritance + cascade config
    │   └── DbInitializer.cs    # Seed data (dealerships, users, vehicles)
    └── Migrations/             # EF Core migration history (committed to source control)
```

## Notes on Design Decisions

- **Minimal APIs over Controllers**: chosen initially for its closer resemblance to Express.js route handlers, making the transition from a Node.js background more intuitive while learning ASP.NET Core fundamentals.
- **User Secrets over `.env`**: .NET doesn't use `.env` files natively; User Secrets is the framework-native equivalent for local development, storing sensitive config outside the project directory entirely so it can never accidentally be committed.
- **Migrations are committed to git**: unlike build artifacts (`bin/`, `obj/`), EF Core migrations are source code describing schema history and are required for anyone cloning the repo to set up a working database.
- **No server-side token storage**: JWTs are stateless by design — the tradeoff is that tokens can't be revoked before they expire without adding extra infrastructure (e.g. a token blocklist). Acceptable for this project's current scope; noted as a possible future improvement alongside refresh tokens.
- **DTOs instead of binding directly to entities**: Create/Update endpoints accept purpose-built request records rather than the EF Core entity types directly, so clients can never set server-controlled fields (`Id`, `Created`, password hashes, etc.) through the request body.

## License

This is a personal learning/portfolio project. Feel free to browse or fork for your own learning purposes.