using Microsoft.AspNetCore.Razor.TagHelpers;
using PersonalBrand.Infrastructure;

namespace PersonalBrand.Admin.TagHelpers;

[HtmlTargetElement("textarea", Attributes = "rich-editor")]
public sealed class RichEditorTagHelper(SafeMarkdown renderer) : TagHelper
{
    public override int Order => 1000;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = output.Content.IsModified ? output.Content.GetContent() : (await output.GetChildContentAsync()).GetContent();
        output.Attributes.RemoveAll("rich-editor");
        output.Attributes.SetAttribute("data-rich-editor", "true");
        output.Attributes.SetAttribute("data-initial-html", renderer.Render(System.Net.WebUtility.HtmlDecode(content)));
    }
}
