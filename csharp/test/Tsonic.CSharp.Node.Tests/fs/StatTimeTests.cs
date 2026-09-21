using System.Reflection;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class StatTimeTests
{
    private static readonly Func<DateTime, double> ConvertTime = Type
        .GetType("Tsonic.CSharp.Node.StatTime, Tsonic.CSharp.Node", throwOnError: true)!
        .GetMethod("ToUnixMilliseconds", BindingFlags.Public | BindingFlags.Static)!
        .CreateDelegate<Func<DateTime, double>>();

    public static IEnumerable<object[]> Boundaries()
    {
        var epoch = DateTime.UnixEpoch.Ticks;
        var values = new[]
        {
            DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks,
            epoch - 10001, epoch - 10000, epoch - 9999, epoch - 1,
            epoch, epoch + 1, epoch + 9999, epoch + 10000, epoch + 10001,
            new DateTime(2024, 2, 29, 12, 34, 56).Ticks + 9876543,
            new DateTime(2024, 7, 1, 12, 34, 56).Ticks + 1234567,
            new DateTime(2024, 1, 1, 12, 34, 56).Ticks + 1234567,
        };
        foreach (var kind in new[] { DateTimeKind.Utc, DateTimeKind.Local, DateTimeKind.Unspecified })
            foreach (var ticks in values)
                yield return new object[] { ticks, kind };
    }

    [Theory]
    [MemberData(nameof(Boundaries))]
    public void ConversionMatchesNativeUtcNormalizationAndFloor(long ticks, DateTimeKind kind)
    {
        var value = new DateTime(ticks, kind);
        var expected = new DateTimeOffset(value.ToUniversalTime()).ToUnixTimeMilliseconds();
        Assert.Equal((double)expected, ConvertTime(value));
    }

    [Fact]
    public void SubmillisecondBeforeEpochFloorsInsteadOfTruncating()
    {
        Assert.Equal(-1, ConvertTime(DateTime.UnixEpoch.AddTicks(-1)));
        Assert.Equal(-2, ConvertTime(DateTime.UnixEpoch.AddTicks(-10001)));
        Assert.Equal(0, ConvertTime(DateTime.UnixEpoch.AddTicks(9999)));
    }

    [Fact]
    public void ConversionAllocatesNothing()
    {
        var value = DateTime.UnixEpoch.AddTicks(-1);
        for (var index = 0; index < 1000; index++) ConvertTime(value);
        var before = GC.GetAllocatedBytesForCurrentThread();
        var result = 0.0;
        for (var index = 0; index < 1000; index++) result += ConvertTime(value);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(-1000, result);
        Assert.Equal(0, allocated);
    }
}
