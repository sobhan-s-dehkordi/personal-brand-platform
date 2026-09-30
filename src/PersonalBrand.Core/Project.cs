using System.ComponentModel.DataAnnotations;

namespace PersonalBrand.Core;

public sealed class Project : Content
{
    public string Problem { get; set; } = "";
    public string Solution { get; set; } = "";
    public string ArchitectureDescription { get; set; } = "";
    public string Challenges { get; set; } = "";
    public string Results { get; set; } = "";

    [Url]
    public string? GitHubUrl
    {
        get; set;
    }

    [Url]
    public string? LiveDemoUrl
    {
        get; set;
    }
    public int DisplayOrder
    {
        get; set;
    }
    public List<Technology> Technologies { get; set; } = [];
    public List<ProjectImage> Screenshots { get; set; } = [];
}
