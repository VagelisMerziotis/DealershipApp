namespace DealershipApi.Models;
using System.Text.Json.Serialization;

public class User
{
    public int Id { get; set; }
    public int DealershipId { get; set; }
    public Dealership Dealership { get; set; }
    public string Role { get; set; }
    public string Username { get; set; }
    [JsonIgnore]
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public int Salary { get; set; }
    public bool IsStakeholder { get; set; }
}