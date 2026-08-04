namespace DealershipApi.Models;

public class Automobile : Vehicle
{
    public string? SideOfSteering { get; set; }
    public int? DoorsNumber { get; set; }
    public bool HasStorage { get; set; }
    public decimal StorageSize { get; set; }
    public bool HasCrashedOnce { get; set; }
    public int Gears { get; set; }
}