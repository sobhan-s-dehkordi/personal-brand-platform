using System.ComponentModel.DataAnnotations;

namespace PersonalBrand.Core;

public enum ReservationStatus
{
    PendingPayment,
    PaymentSubmitted,
    Confirmed,
    Rejected,
    Expired,
    Cancelled
}
