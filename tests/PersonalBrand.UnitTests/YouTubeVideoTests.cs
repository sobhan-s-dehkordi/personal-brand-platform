using PersonalBrand.Core;

namespace PersonalBrand.UnitTests;

public sealed class YouTubeVideoTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=lOKD08QAhyc")]
    [InlineData("https://youtu.be/lOKD08QAhyc?t=20")]
    [InlineData("https://www.youtube.com/shorts/lOKD08QAhyc")]
    [InlineData("https://www.youtube-nocookie.com/embed/lOKD08QAhyc")]
    [InlineData("lOKD08QAhyc")]
    public void Normalizes_supported_video_links(string input) => Assert.Equal("lOKD08QAhyc", YouTubeVideo.Parse(input));
    [Theory]
    [InlineData("https://youtube.com.evil.test/watch?v=lOKD08QAhyc")]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://youtube.com/watch?v=invalid")]
    [InlineData("")]
    public void Rejects_invalid_or_untrusted_links(string input) => Assert.Null(YouTubeVideo.Parse(input));
}
