using System.ComponentModel.DataAnnotations;

namespace PersonalBrand.Core;

public sealed class Course : Content
{
    public string Difficulty { get; set; } = "Intermediate";
    public int EstimatedDuration
    {
        get; set;
    }
    public List<Lesson> Lessons { get; set; } = [];
}
