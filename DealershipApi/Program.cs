using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DealershipApi.Data;
using DealershipApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

app.MapGet("/api/vehicles", async (AppDbContext db) =>
{
    var vehicles = await db.Vehicles.ToListAsync();
    return Results.Ok(vehicles);

}).RequireAuthorization();

app.MapPost("/api/orderCar", async (ClaimsPrincipal reqUser, AppDbContext db, Automobile newCar) =>
{
    // Check if the employ works in that dealership
    var role = reqUser.FindFirst(ClaimTypes.Role)?.Value;
    if (role != "Admin") return Results.Unauthorized();
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

record LoginRequest(string Username, string Password);
