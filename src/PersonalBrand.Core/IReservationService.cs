namespace PersonalBrand.Core;

public interface IReservationService
{
    Task<WorkshopReservation> ReserveAsync(Guid workshop, ParticipantInput input, CancellationToken ct);
    Task SubmitAsync(string token, string tracking, Stream stream, string name, CancellationToken ct);
    Task ReviewAsync(Guid id, string action, string admin, string note, CancellationToken ct);
}
