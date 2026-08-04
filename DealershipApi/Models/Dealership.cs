namespace DealershipApi.Models;

public class Dealership
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Number { get; set; }
    public int Zipcode { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string? Website { get; set; }
    public decimal SqFeet { get; set; }
    public decimal Budget { get; set; }
    public DateTime Created { get; init; }
    public DateTime Modified { get; set; }
    // Initialize lists of users and vehicles as empty
    public List<User> Users { get; set; } = new(); 
    public List<Vehicle> Vehicles { get; set; } = new();
}