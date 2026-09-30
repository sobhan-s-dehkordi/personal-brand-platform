namespace PersonalBrand.Core;

public sealed class StorageOptions
{
    public string Root { get; set; } = "App_Data/uploads";
    public int MaxBytes { get; set; } = 5 * 1024 * 1024;
}
