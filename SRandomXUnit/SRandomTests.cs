using SuperRandom;
using System;
using Xunit;


namespace SRandomXUnit {
    public class SRandomTests {

        [Theory]
        [InlineData(0, 1, 0)]
        [InlineData(1, 2, 1)]
        [InlineData(ulong.MaxValue-1, ulong.MaxValue, ulong.MaxValue - 1)]
        public void RangeTest(ulong min, ulong max, ulong expected) {
            Assert.Equal(expected, SRandom.Next(min, max));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(ulong.MaxValue, ulong.MaxValue)]
        public void RangeTestException(ulong min, ulong max) {
            Assert.Throws<ArgumentException>(() => SRandom.Next(min, max));
        }

    }
}
