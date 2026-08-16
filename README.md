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
- [x] Full CRUD endpoints for Users
- [x] Full CRUD endpoints for Vehicles
- [x] Password hashes excluded from all API responses (`[JsonIgnore]` on `User.PasswordHash`)
- [x] Cascade delete — removing a Dealership removes its Users and Vehicles
- [ ] Endpoint definitions split across multiple files (currently all in `Program.cs`)
- [ ] Consistent 404-vs-400 responses across all endpoints (see Known Issues)
- [ ] Consistent REST-style route naming (see Known Issues)
- [ ] Remove duplicate vehicle-listing endpoint (see Known Issues)
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

### Password hash safety

`User.PasswordHash` is decorated with `[JsonIgnore]`, so it is never present in any JSON response — regardless of which endpoint returns a `User` object, or whether that endpoint remembers to manually project the response down to a safe field list. This was fixed after an early version of `GET /api/getAllUsers` and `GET /api/getUser/{id}` briefly returned the full entity, hash included; rather than relying on every endpoint to manually exclude the field, the property itself is now unrepresentable in JSON. `[JsonIgnore]` only affects serialization — server-side code (e.g. `BCrypt.Verify` during login) still reads and writes the property normally.

### Authorization model

Authorization combines two mechanisms:

- **Role-based** — endpoints restricted via `RequireAuthorization(policy => policy.RequireRole(...))`, e.g. only `Admin` can delete a dealership.
- **Dealership-scoped** — for list endpoints on Vehicles and Users, non-Admin/Manager roles only see data belonging to their own dealership (read from the `dealershipId` claim in their token, or looked up server-side), while `Admin` and `Manager` can access all dealerships' data.

| Resource | Endpoint | Who | Scope |
|---|---|---|---|
| Login | `POST /api/login` | Anyone | — |
| Vehicles | `GET /api/vehicles` | Any authenticated user | Admin/Manager see all; others see only their own dealership |
| Vehicles | `GET /api/getCar/{id}` | Any authenticated user | Not scoped — any authenticated user can fetch any vehicle by ID (see Known Issues) |
| Vehicles | `POST /api/orderCar` | Admin only | Scoped to the Admin's own dealership |
| Vehicles | `PUT /api/modifyCar/{id}` | Admin only | — |
| Vehicles | `DELETE /api/deleteCar/{id}` | Admin only | — |
| Dealerships | `GET /api/getDealership/{id}` | Admin, Manager | Any dealership |
| Dealerships | `GET /api/getAllDealerships` | Admin, Manager | All |
| Dealerships | `POST /api/createDealership` | Admin only | — |
| Dealerships | `PUT /api/modifyDealership/{id}` | Admin only | — |
| Dealerships | `DELETE /api/deleteDealership/{id}` | Admin only | Cascades to Users/Vehicles |
| Users | `GET /api/getAllUsers` | Admin, Manager | Admin sees all; Manager sees only their own dealership's staff |
| Users | `GET /api/getUser/{id}` | Admin, Manager | Any user |
| Users | `POST /api/createUser` | Admin, Manager | — |
| Users | `PUT /api/modifyUser/{id}` | Admin, Manager | — |
| Users | `DELETE /api/deleteUser/{id}` | Admin, Manager | — |

## CRUD Patterns

Every Create/Update endpoint follows the same shape, established with Dealerships and reused for Users and Vehicles:

- **Create** — accepts a purpose-built `Create...Request` DTO rather than the raw entity, so clients can never set server-controlled fields (`Id`, `Created`, `PasswordHash`, etc.) through the request body. For Users specifically, the DTO takes a plaintext `Password` field, which is hashed with BCrypt before the `User` entity is constructed — this transformation is why user creation is written out field-by-field rather than via the reflection helper described below.
- **Update** — accepts a `Modify...Request` DTO where every field is nullable, representing a partial update. Instead of a long chain of manual `if (request.X is not null) entity.X = request.X;` checks, these endpoints use **reflection** (`System.Reflection.PropertyInfo`) to loop over the DTO's properties, skip any left `null` by the client, and copy the rest onto the matching property of the tracked entity by name. This keeps the update endpoints short, at the cost of type safety: a property renamed on one side without the other fails silently at runtime instead of at compile time. `ModifyUserRequest` deliberately excludes `Password` — password changes are sensitive enough to warrant their own dedicated endpoint rather than being foldable into a generic patch. For `Vehicle`, the reflection loop matches properties against `vehicle.GetType()` (the concrete runtime type, e.g. `Automobile`) rather than the abstract `Vehicle` base type, so subtype-specific fields like `Gears` or `DoorsNumber` are found correctly.
- **Delete** — uses EF Core's `ExecuteDeleteAsync()` for a direct, single-round-trip database delete rather than loading the entity first, where it doesn't need to be loaded for validation reasons.

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
    │   ├── User.cs             # PasswordHash marked [JsonIgnore]
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
- **Reflection for partial updates, explicit assignment for creation**: partial-update endpoints copy fields generically via reflection since it's a pure optional-overwrite operation; creation endpoints (especially Users, which need password hashing) are written out explicitly, since that step involves real transformation logic that a generic name-matching copy can't safely express.
- **`[JsonIgnore]` over manual response projection for secrets**: rather than remembering to exclude `PasswordHash` in every individual endpoint's response, it's excluded once at the model level — a mistake at one call site can't reintroduce the leak.

## Known Issues / Cleanup Backlog

- **Duplicate vehicle-listing endpoints**: `GET /api/vehicles` and `GET /api/getAllVehicles` both return a dealership-scoped vehicle list with near-identical logic. One should be removed.
- **`GET /api/getCar/{id}` is not dealership-scoped**: unlike the list endpoint, fetching a single vehicle by ID does not check the caller's role or dealership, so any authenticated user can currently look up any vehicle regardless of which dealership it belongs to.
- **`PUT /api/modifyUser/{id}` returns an empty `200 OK`** rather than the updated user, unlike every other Update endpoint in the API, which return the modified resource.
- **`DELETE /api/deleteUser/{id}` does not check whether a user was actually deleted** before returning success, and is currently reachable by both `Admin` and `Manager` roles rather than `Admin` only as originally intended.
- Several "not found" cases still return `400 BadRequest` instead of the more correct `404 NotFound` (e.g. `getDealership`, `orderCar`).
- Route naming is inconsistent across resources (e.g. `getDealership` vs `getAllUsers` vs `getUser`) rather than following a uniform REST convention (`GET /api/dealerships`, `GET /api/dealerships/{id}`, etc.) — a rename pass is planned.

## License

This is a personal learning/portfolio project. Feel free to browse or fork for your own learning purposes.