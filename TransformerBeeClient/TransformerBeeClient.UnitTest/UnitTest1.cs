using FluentAssertions;
using Xunit;

namespace TransformerBeeClient.UnitTest;

public class Tests
{
    [Fact]
    public void Test1()
    {
        true.Should().BeTrue();
    }
}
