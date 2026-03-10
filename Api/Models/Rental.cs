namespace Api.Models;

public class Rental
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CarId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public DateTimeOffset? ReturnedAt { get; set; }
    public RentalStatus Status { get; set; }
    public int? KilometersDriven { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public Car Car { get; set; } = null!;
}
