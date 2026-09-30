using System.ComponentModel.DataAnnotations;

namespace PersonalBrand.Core;

public abstract class Content : Entity
{
    public bool IsVisible { get; set; } = true;
    [Required, RegularExpression("^(en|fa)$")]
    public string Language { get; set; } = "en";

    [Required, MaxLength(160), RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string Slug { get; set; } = "";
    public Guid? TranslationGroupId
    {
        get; set;
    }

    [Required, MaxLength(240)]
    public string Title { get; set; } = "";

    [MaxLength(1000)]
    public string Summary { get; set; } = "";
    public string Body { get; set; } = "";
    public string? CoverImage
    {
        get; set;
    }
    public ContentStatus Status
    {
        get; set;
    }
    public DateTimeOffset? PublishedAt
    {
        get; set;
    }
    public DateTimeOffset UpdatedAt
    {
        get; set;
    }
    public bool IsFeatured
    {
        get; set;
    }

    [MaxLength(240)]
    public string? SeoTitle
    {
        get; set;
    }

    [MaxLength(500)]
    public string? SeoDescription
    {
        get; set;
    }
}
