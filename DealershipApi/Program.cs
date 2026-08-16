using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DealershipApi.Data;
using DealershipApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register the middleware for Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>((options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Register the implicit middleware for Auth 
app.UseAuthentication();
app.UseAuthorization();

// Custom scope for DB initialization
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbInitializer.Seed(db);
}

app.MapPost("/api/login", async (LoginRequest request, AppDbContext db) =>
{
    // Db calls are always asynchronous
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
    if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return Results.Unauthorized();
    
    // Create a set of claims for using in the JWT 
    var claims = new[]
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("dealershipId", user.DealershipId.ToString()),
    };

    // Create the Symmetric Key and credentials
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // Fabricate the JWT object and stringify it 
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddHours(6),
        signingCredentials: creds);
    string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    
    return Results.Ok(new { token = tokenString });
});

app.MapGet("/api/vehicles", async (AppDbContext db, ClaimsPrincipal reqUser) =>
{
    if (!int.TryParse(reqUser.FindFirst("dealershipId")?.Value, out int userDealershipId)) return Results.Unauthorized();
    var vehicles = (await db.Vehicles.ToListAsync()).FindAll(vehicle => vehicle.DealershipId == userDealershipId);
    
    return Results.Ok(vehicles);
}).RequireAuthorization();

app.MapPost("/api/orderCar", async (ClaimsPrincipal reqUser, AppDbContext db, Automobile newCar) =>
{
    // Extract dealershpId from the user object in request.
    var dealershipIdClaim = reqUser.FindFirst("dealershipId")?.Value;
    if (dealershipIdClaim is null) return Results.BadRequest();
    var dealership = await db.Dealerships.FirstOrDefaultAsync(d => d.Id == int.Parse(dealershipIdClaim));
    // Check if dealership has enough budget and modify newCar before storing
    if (dealership is null) return Results.BadRequest("No dealership found from employee");
    newCar.DealershipId = dealership.Id;
    newCar.Dealership = dealership;
    newCar.Modified = DateTime.UtcNow;
    newCar.Created =  DateTime.UtcNow;
    if (dealership.Budget - newCar.Price < 0 ) return Results.BadRequest("Not enough budget");
    // Modify budget and add car
    db.Vehicles.Add(newCar);
    dealership.Budget -= newCar.Price;
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        newCar.Id,
        newCar.Brand,
        newCar.Model,
        newCar.Price,
        newCar.DealershipId
    });

}).RequireAuthorization(policy => policy.RequireRole("Admin"));

// Dealership APIs
app.MapGet("/api/getDealership/{dealershipId}", async (AppDbContext db, int dealershipId) =>
{
    var dealership = await db.Dealerships.FindAsync(dealershipId);
    if (dealership is null) return Results.BadRequest("No dealership found");
    return Results.Ok(new
    {
        dealership.Name,
        dealership.Budget,
        dealership.Users,
        dealership.Address,
        dealership.City,
        dealership.State,
    });

}).RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"));

app.MapGet("/api/getAllDealerships", async (AppDbContext db) =>
{
    return await db.Dealerships.ToListAsync();
}).RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"));

app.MapPost("/api/createDealership", async (AppDbContext db, CreateDealershipRequest request) =>
{
    Dealership dealership = new Dealership
    {
        Name = request.Name,
        Address = request.Address,
        Number = request.Number,
        Zipcode = request.Zipcode,
        City = request.City,
        State = request.State,
        Country = request.Country,
        Phone = request.Phone,
        Email = request.Email,
        Website = request.Website,
        SqFeet = request.SqFeet,
        Budget = request.Budget,
        Created = DateTime.UtcNow,
        Modified = DateTime.UtcNow
    };
    await db.Dealerships.AddAsync(dealership);
    await db.SaveChangesAsync();
    
    return Results.Ok(dealership);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/api/modifyDealership/{dealershipId}", async (int dealershipId, AppDbContext db, ModifyDealershipRequest request) =>
{   
    if (dealershipId <= 0) return Results.BadRequest("Invalid dealership ID.");
    var dealership = await db.Dealerships.FirstOrDefaultAsync(d => d.Id == dealershipId);
    if (dealership is null) return Results.NotFound("No dealership found");

    foreach (var requestProp in typeof(ModifyDealershipRequest).GetProperties())
    {
        var newValue = requestProp.GetValue(request);
        if (newValue is null) continue;

        PropertyInfo? targetProp = typeof(Dealership).GetProperty(requestProp.Name);
        if (targetProp is not null && targetProp.CanWrite) targetProp.SetValue(dealership, newValue);
    }

    dealership.Modified = DateTime.UtcNow;
    await db.SaveChangesAsync();
    
    return Results.Ok(dealership);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/api/deleteDealership/{dealershipId}", async (int dealershipId, AppDbContext db) =>
{
    if (dealershipId <= 0) return Results.BadRequest("Invalid dealershipId");
    int rowsDeleted = await db.Dealerships
        .Where(d => d.Id == dealershipId)
        .ExecuteDeleteAsync();
    
    if (rowsDeleted == 0) return Results.NotFound("Nothing was found to be deleted");
    return Results.Ok($"Deleted dealership with ID: {dealershipId} successfully!");
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

// Users APIs
app.MapGet("/api/getAllUsers", async (AppDbContext db, ClaimsPrincipal reqUser) =>
{
    var reqUsername = reqUser.FindFirst(ClaimTypes.Name)?.Value;
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == reqUsername);
    if (user is null) return Results.BadRequest($"No role found for user {reqUser.FindFirst(ClaimTypes.Name)?.Value}");
    if (user.Role ==  "Admin")
    {
        var users = await db.Users.ToListAsync();
        return Results.Ok(users);
    }

    if (user.Role == "Manager")
    {
        var users = await db.Users
            .Where(u => u.DealershipId == user.DealershipId)
            .ToListAsync();
        return Results.Ok(users);
    }

    return Results.BadRequest("User is not a manager/admin or does not exist.");
}).RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"));

app.MapGet("/api/getUser/{userId}", async (AppDbContext db, int userId) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    if (user is null) return Results.NotFound($"No user found with id {userId}");
    
    return Results.Ok(new
    {
        user.Username,
        user.Email,
        user.FirstName,
        user.LastName,
        user.Role,
    });
}).RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"));

app.MapPost("/api/createUser", async (AppDbContext db, CreateUserRequest newUser) =>
{
    bool exists = await db.Dealerships.AnyAsync(d => d.Id == newUser.DealershipId);
    if (newUser.DealershipId < 0 || !exists)
    {
        return Results.BadRequest("Invalid dealership ID.");
    }
    
    var user = new User
    {
        DealershipId = newUser.DealershipId,
        Role = newUser.Role,
        Username = newUser.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.Password),
        Email = newUser.Email,
        FirstName = newUser.FirstName,
        LastName = newUser.LastName,
        Address = newUser.Address,
        City = newUser.City,
        State = newUser.State,
        Country = newUser.Country,
        Salary = newUser.Salary,
        IsStakeholder = newUser.IsStakeholder
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        user.Username,
        user.Email,
        user.FirstName,
        user.LastName,
    });

}).RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"));

app.MapPut("/api/modifyUser/{userId}", async (int userId, AppDbContext db, ModifyUserRequest request) =>
{
    if (userId <= 0) return Results.BadRequest("Invalid dealership ID.");
    var user = await db.Users.FindAsync(userId);
    if (user is null) return Results.BadRequest("User not found.");

    foreach (var requestProp in typeof(ModifyUserRequest).GetProperties())
    {
        var newValue = requestProp.GetValue(request);
        if (newValue is null) continue;

        PropertyInfo? targetProp = typeof(User).GetProperty(requestProp.Name);
        if (targetProp is null) continue;
        targetProp.SetValue(user, newValue);
    }
    
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"));

// Users APIs
app.MapGet("/api/getCar/{carId}", (int carId, AppDbContext db) =>
{
    if  (carId <= 0) return Results.BadRequest("Invalid dealership ID.");
    var vehicle = db.Vehicles
        .Where(v => v.Id == carId);
    return Results.Ok(vehicle);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

record LoginRequest(string Username, string Password);
record CreateDealershipRequest(
    string Name,
    string Address,
    string Number,
    int Zipcode,
    string City,
    string State,
    string Country,
    string Phone,
    string Email,
    string? Website,
    decimal SqFeet,
    decimal Budget
);

record ModifyDealershipRequest(
    string? Name,
    string? Phone,
    string? Email,
    string? Website,
    decimal? SqFeet,
    decimal? Budget
);

record CreateUserRequest(
    int DealershipId,
    string Role,
    string Username,
    string Password,
    string Email,
    string FirstName,
    string LastName,
    string Address,
    string City,
    string State,
    string Country,
    int Salary,
    bool IsStakeholder
);

record ModifyUserRequest(
    int? DealershipId,
    string? Role,
    string? Username,
    string? Email,
    string? FirstName,
    string? LastName,
    string? Address,
    string? City,
    string? State,
    string? Country,
    int? Salary,
    bool? IsStakeholder
);

record ModifyVehicleRequest(
    string? Brand,
    string? Model,
    string? Color,
    string? Type,
    string? Description,
    int? YearOfManufacture,
    decimal? Price,
    decimal? EngineVolume,
    bool? Used,
    string? SideOfSteering,
    int? DoorsNumber,
    bool? HasStorage,
    decimal? StorageSize,
    bool? HasCrashedOnce,
    int? Gears
);