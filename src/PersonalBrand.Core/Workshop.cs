using System.ComponentModel.DataAnnotations;

namespace PersonalBrand.Core;

public sealed class Workshop : Content
{
    [Range(1, 1000)]
    public int Capacity { get; set; } = 7;

    [Range(0, 1000000000000d)]
    public decimal Price
    {
        get; set;
    }

    [Required, MaxLength(10)]
    public string Currency { get; set; } = "IRR";
    public DateTimeOffset RegistrationOpensAt
    {
        get; set;
    }
    public DateTimeOffset RegistrationClosesAt
    {
        get; set;
    }
    public DateTimeOffset StartDate
    {
        get; set;
    }
    public DateTimeOffset EndDate
    {
        get; set;
    }
    public string TimeZoneId { get; set; } = "Asia/Tehran";
    public WorkshopStatus WorkshopStatus
    {
        get; set;
    }
    public string Prerequisites { get; set; } = "";
    public string WhatYouWillBuild { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Platform { get; set; } = "";
    public List<WorkshopSession> Sessions { get; set; } = [];

    public bool CanReserve(DateTimeOffset now, int occupied) => IsVisible && Status == ContentStatus.Published && WorkshopStatus == WorkshopStatus.Open && now >= RegistrationOpensAt && now < RegistrationClosesAt && occupied < Capacity;
}
