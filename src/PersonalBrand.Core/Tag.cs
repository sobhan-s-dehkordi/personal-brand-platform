using System.ComponentModel.DataAnnotations;

namespace PersonalBrand.Core;

public sealed class Tag : Entity
{
    [MaxLength(80)]
    public string Name { get; set; } = "";
}
