using System.Text.RegularExpressions;

namespace PersonalBrand.Core;

public static class YouTubeVideo
{
    public static string? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;
        var value = input.Trim();
        if (Regex.IsMatch(value, "^[a-zA-Z0-9_-]{11}$"))
            return value;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http"))
            return null;
        string? id = null;
        var parts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (uri.Host is "youtu.be" && parts.Length == 1)
            id = parts[0];
        else if (uri.Host is "youtube.com" or "www.youtube.com" or "m.youtube.com" or "www.youtube-nocookie.com")
        {
            if (parts.Length == 2 && parts[0] is "embed" or "shorts" or "live")
                id = parts[1];
            else if (uri.AbsolutePath == "/watch")
                id = uri.Query.TrimStart('?').Split('&').Select(x => x.Split('=', 2)).Where(x => x.Length == 2 && x[0] == "v").Select(x => Uri.UnescapeDataString(x[1])).FirstOrDefault();
        }

        return id is not null && Regex.IsMatch(id, "^[a-zA-Z0-9_-]{11}$") ? id : null;
    }
}
