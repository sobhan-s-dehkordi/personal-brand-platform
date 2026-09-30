using System.Net;
using System.Net.Mail;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalBrand.Core;
using Ganss.Xss;
using Markdig;

namespace PersonalBrand.Infrastructure;

public sealed class LocalFileStorage(IOptions<StorageOptions> options) : IFileStorage
{
    private string Root => Path.GetFullPath(options.Value.Root);

    private string FilePath(string key, bool isPublic)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(key, "^[a-f0-9]{32}\\.(png|jpg|webp)$"))
            throw new DomainException("Invalid file key.");
        return Path.Combine(Root, isPublic ? "public" : "private", key);
    }

    public async Task<StoredFile> StoreAsync(Stream input, string name, bool isPublic, CancellationToken ct)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await input.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + read > options.Value.MaxBytes)
                throw new DomainException("Image exceeds the 5 MB limit.");
            await buffer.WriteAsync(chunk.AsMemory(0, read), ct);
        }

        var bytes = buffer.ToArray();
        var ext = Path.GetExtension(name).ToLowerInvariant();
        var png = bytes.Length > 24 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var jpg = bytes.Length > 4 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255;
        var webp = bytes.Length > 16 && System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" && System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP";
        if (!(png && ext == ".png" || jpg && ext is ".jpg" or ".jpeg" || webp && ext == ".webp"))
            throw new DomainException("Upload a valid PNG, JPEG, or WebP image.");
        using var encoded = SkiaSharp.SKData.CreateCopy(bytes);
        using var codec = SkiaSharp.SKCodec.Create(encoded);
        if (codec is null || codec.Info.Width <= 0 || codec.Info.Height <= 0 || (long)codec.Info.Width * codec.Info.Height > 16000000)
            throw new DomainException("Invalid image or dimensions exceed 16 megapixels.");
        using var bitmap = new SkiaSharp.SKBitmap(codec.Info.Width, codec.Info.Height);
        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SkiaSharp.SKCodecResult.Success)
            throw new DomainException("The image is incomplete or damaged.");
        using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
        using var clean = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        bytes = clean.ToArray();
        var key = Guid.NewGuid().ToString("N") + ".png";
        var path = FilePath(key, isPublic);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, bytes, ct);
        return new StoredFile(key, "image/png", bytes.Length);
    }

    public Task<Stream> OpenPrivateAsync(string key, CancellationToken ct) => Task.FromResult<Stream>(File.OpenRead(FilePath(key, false)));
    public Task<Stream> OpenPublicAsync(string key, CancellationToken ct) => Task.FromResult<Stream>(File.OpenRead(FilePath(key, true)));
    public Task DeletePrivateAsync(string key, CancellationToken ct)
    {
        File.Delete(FilePath(key, false));
        return Task.CompletedTask;
    }
}
