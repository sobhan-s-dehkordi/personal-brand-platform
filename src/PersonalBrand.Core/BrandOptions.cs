namespace PersonalBrand.Core;

public sealed class BrandOptions
{
    public string Name { get; set; } = "Your Name";
    public string ProfessionalTitle { get; set; } = "Backend Engineer";
    public string ShortBio { get; set; } = "I build software, write about engineering, and teach practical development.";
    public string? ProfileImage
    {
        get; set;
    }
    public string? GitHubUrl
    {
        get; set;
    }
    public string? LinkedInUrl
    {
        get; set;
    }
    public string? YouTubeUrl
    {
        get; set;
    }
    public string ContactEmail { get; set; } = "";
    public string? ResumePath
    {
        get; set;
    }
    public string DefaultSeoDescription { get; set; } = ".NET, AI, and software architecture. Projects, articles, and practical courses.";
}
