using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class UtilTypesTests
{
    [Fact]
    public void TypePredicates_DetectSupportedClosedCarriers()
    {
        Assert.True(util_types.isArrayBuffer(new byte[] { 1 }));
        Assert.True(util_types.isTypedArray(new int[] { 1 }));
        Assert.True(util_types.isPromise(Task.CompletedTask));
        Assert.True(util_types.isDate(System.DateTime.UtcNow));
        Assert.True(util_types.isMap(new Dictionary<string, string>()));
        Assert.True(util_types.isRegExp(new Regex("x")));
    }

    [Fact]
    public void TypePredicates_ReturnFalseForMismatches()
    {
        Assert.False(util_types.isArrayBuffer("x"));
        Assert.False(util_types.isPromise("x"));
        Assert.False(util_types.isRegExp("x"));
    }
}
