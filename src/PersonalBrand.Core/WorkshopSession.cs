using System.ComponentModel.DataAnnotations;

namespace PersonalBrand.Core;

public sealed class WorkshopSession : Entity
{
    public Guid? TranslationGroupId { get; set; }
    public bool IsVisible { get; set; } = true;
    public Guid WorkshopId
    {
        get; set;
    }

    [Required, MaxLength(240)]
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int SessionNumber
    {
        get; set;
    }
    public DateTimeOffset StartAt
    {
        get; set;
    }
    public DateTimeOffset EndAt
    {
        get; set;
    }
}
