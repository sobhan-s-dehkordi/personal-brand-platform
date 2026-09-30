using System.ComponentModel.DataAnnotations;

namespace PersonalBrand.Core;

public sealed class Article : Content
{
    public int ReadingTime { get; set; } = 5;
    public List<Tag> Tags { get; set; } = [];
    public List<Course> RelatedCourses { get; set; } = [];
}
