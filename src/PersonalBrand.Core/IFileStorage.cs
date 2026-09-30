namespace PersonalBrand.Core;

public interface IFileStorage
{
    Task<StoredFile> StoreAsync(Stream input, string name, bool isPublic, CancellationToken ct);
    Task<Stream> OpenPrivateAsync(string key, CancellationToken ct);
    Task<Stream> OpenPublicAsync(string key, CancellationToken ct);
    Task DeletePrivateAsync(string key, CancellationToken ct);
}
