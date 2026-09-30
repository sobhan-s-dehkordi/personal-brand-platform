using PersonalBrand.Infrastructure;

namespace PersonalBrand.IntegrationTests;

public sealed class RichContentTests
{
    [Fact]
    public void Rich_content_keeps_emphasis_but_strips_theme_overrides_and_active_content()
    {
        var renderer = new SafeMarkdown();
        var html = renderer.Render("pb-html:<p class='large' style='color:red;font-size:99px;font-family:Comic Sans' onclick='alert(1)'><strong>Bold</strong><u>Underlined</u><em>Italic</em><s>Deleted</s><a href='javascript:alert(1)'>Link</a></p><script>alert(1)</script><iframe src='https://evil.test'></iframe><h1>Size</h1>");
        Assert.Contains("<strong>Bold</strong>", html);
        Assert.Contains("<u>Underlined</u>", html);
        foreach (var denied in new[]
        {
            "style=",
            "class=",
            "onclick",
            "javascript:",
            "<script",
            "<iframe",
            "<h1"
        }

        )
            Assert.DoesNotContain(denied, html);
        Assert.Equal("Hello", renderer.PlainText("pb-html:<p><strong>Hello</strong></p>"));
        Assert.Contains("<h2>Existing Markdown</h2>", renderer.Render("## Existing Markdown"));
    }
}
