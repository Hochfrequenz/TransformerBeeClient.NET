using FluentAssertions;
using Xunit;

namespace TransformerBeeClient.IntegrationTest;

public class Tests
{
    [Fact]
    public void Test1()
    {
        true.Should().BeTrue();
    }
}
