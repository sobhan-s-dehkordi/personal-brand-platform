using System.ComponentModel.DataAnnotations;

namespace PersonalBrand.Core;

public sealed class Lesson : Entity
{
    public Guid? TranslationGroupId { get; set; }
    public Guid CourseId
    {
        get; set;
    }
    public Course Course { get; set; } = null!;

    [Required, MaxLength(160), RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string Slug { get; set; } = "";

    [Required, MaxLength(240)]
    public string Title { get; set; } = "";
    public int Order
    {
        get; set;
    }

    [RegularExpression("^[a-zA-Z0-9_-]{11}$")]
    public string? YouTubeVideoId
    {
        get; set;
    }
    public string? YouTubeUrl
    {
        get; set;
    }
    public int Duration
    {
        get; set;
    }
    public string Summary { get; set; } = "";
    public string ContentBody { get; set; } = "";
    public string KeyPoints { get; set; } = "";
    public string Resources { get; set; } = "";

    [Url]
    public string? GitHubUrl
    {
        get; set;
    }
    public bool IsPublished
    {
        get; set;
    }
}
