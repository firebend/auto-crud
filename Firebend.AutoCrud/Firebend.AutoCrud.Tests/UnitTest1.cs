using NUnit.Framework;
using FluentAssertions;

namespace Firebend.AutoCrud.Tests
{
    public class Tests
    {
        [Test]
        public void TestShouldPass()
        {
            var expected = 1;
            var actual = 1;
            actual.Should().Be(expected);
        }
    }
}