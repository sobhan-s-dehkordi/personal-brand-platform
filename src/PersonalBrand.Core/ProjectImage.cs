using System.ComponentModel.DataAnnotations;

namespace PersonalBrand.Core;

public sealed class ProjectImage : Entity
{
    public Guid ProjectId
    {
        get; set;
    }
    public string Path { get; set; } = "";
    public string Alt { get; set; } = "";
}
