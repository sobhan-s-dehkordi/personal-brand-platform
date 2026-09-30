namespace PersonalBrand.Core;

public interface IAudienceService
{
    Task EnrollAsync(Guid course, ParticipantInput input, CancellationToken ct);
    Task SubscribeAsync(ParticipantInput input, CancellationToken ct);
    Task<bool> UnsubscribeAsync(string token, CancellationToken ct);
}
