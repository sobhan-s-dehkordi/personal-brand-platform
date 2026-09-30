using System.ComponentModel.DataAnnotations;

namespace PersonalBrand.Core;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
