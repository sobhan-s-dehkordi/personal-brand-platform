namespace PersonalBrand.Core;

public sealed class ReservationHistory : Entity
{
    public Guid ReservationId
    {
        get; set;
    }
    public string Action { get; set; } = "";
    public DateTimeOffset OccurredAt
    {
        get; set;
    }
    public string ActorType { get; set; } = "";
    public string ActorIdentifier { get; set; } = "";
    public string? Note
    {
        get; set;
    }
}
