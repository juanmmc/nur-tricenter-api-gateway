using ApiGateway.Discovery;
using Xunit;

namespace ApiGateway.Tests.Discovery;

public class ConsulRoutePollerTests
{
    [Theory]
    [InlineData("15s", 15)]
    [InlineData("30S", 30)]
    [InlineData("1m", 60)]
    [InlineData("2M", 120)]
    public void ParseInterval_Parses_Suffix_Notation(string raw, int expectedSeconds)
    {
        var span = ConsulRoutePoller.ParseInterval(raw, TimeSpan.FromSeconds(99));
        Assert.Equal(expectedSeconds, (int)span.TotalSeconds);
    }

    [Fact]
    public void ParseInterval_Falls_Back_When_Empty()
    {
        var fallback = TimeSpan.FromSeconds(7);
        Assert.Equal(fallback, ConsulRoutePoller.ParseInterval("", fallback));
        Assert.Equal(fallback, ConsulRoutePoller.ParseInterval("   ", fallback));
    }

    [Fact]
    public void ParseInterval_Falls_Back_When_Garbage()
    {
        var fallback = TimeSpan.FromSeconds(11);
        Assert.Equal(fallback, ConsulRoutePoller.ParseInterval("not-a-time", fallback));
    }

    [Fact]
    public void ParseInterval_Accepts_Hours()
    {
        Assert.Equal(TimeSpan.FromHours(1),
            ConsulRoutePoller.ParseInterval("1h", TimeSpan.FromSeconds(1)));
    }
}
