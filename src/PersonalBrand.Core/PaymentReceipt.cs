namespace PersonalBrand.Core;

public sealed class PaymentReceipt : Entity
{
    public Guid ReservationId
    {
        get; set;
    }
    public string TrackingNumber { get; set; } = "";
    public string StorageKey { get; set; } = "";
    public string OriginalFileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long FileSize
    {
        get; set;
    }
    public DateTimeOffset UploadedAt
    {
        get; set;
    }
    public DateTimeOffset? ReviewedAt
    {
        get; set;
    }
    public string? ReviewedByAdminId
    {
        get; set;
    }
    public string? ReviewNotes
    {
        get; set;
    }
}
