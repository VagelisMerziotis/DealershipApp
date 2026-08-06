using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DealershipApi.Data;
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

record LoginRequest(string Username, string Password);
