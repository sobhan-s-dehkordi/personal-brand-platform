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

public sealed class SafeMarkdown
{
    public const string RichPrefix = "pb-html:";
    public string Render(string? markdown)
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
        {
            "p",
            "br",
            "strong",
            "b",
            "em",
            "i",
            "u",
            "s",
            "del",
            "ins",
            "a",
            "ul",
            "ol",
            "li",
            "blockquote",
            "pre",
            "code",
            "h1",
            "h2",
            "h3",
            "h4",
            "hr",
            "table",
            "thead",
            "tbody",
            "tr",
            "th",
            "td"
        }

        )
            sanitizer.AllowedTags.Add(tag);
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.Add("href");
        sanitizer.AllowedAttributes.Add("title");
        sanitizer.AllowedCssProperties.Clear();
        var value = markdown ?? "";
        if (value.StartsWith(RichPrefix, StringComparison.Ordinal))
        {
            // Rich content cannot change typography, including pasted headings.
            foreach (var tag in new[]
            {
                "h1",
                "h2",
                "h3",
                "h4"
            }

            )
                sanitizer.AllowedTags.Remove(tag);
            return sanitizer.Sanitize(value[RichPrefix.Length..]);
        }

        return sanitizer.Sanitize(Markdown.ToHtml(value, new MarkdownPipelineBuilder().DisableHtml().UseAdvancedExtensions().Build()));
    }

    public string PlainText(string? value) => new AngleSharp.Html.Parser.HtmlParser().ParseDocument(Render(value)).Body?.TextContent ?? "";
}
