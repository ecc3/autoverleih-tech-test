namespace API.Models;

public class Car
{
    public Guid Id { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public required string LicensePlate { get; set; }
    public int Year { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<Rental> Rentals { get; set; } = [];
}
