namespace DealershipApi.Models;

public abstract class Vehicle
{
    public int Id { get; set; }
    public int DealershipId { get; set; }
    public Dealership Dealership { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; }
    public int YearOfManufacture { get; set; }
    public decimal Price { get; set; }
    public decimal EngineVolume { get; set; }
    public bool Used { get; set; }
    public DateTime Created { get; init; }
    public DateTime Modified { get; set; }
} 