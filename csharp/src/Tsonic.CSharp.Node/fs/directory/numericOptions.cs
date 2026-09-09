using System.Text;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    private static long RequireNonNegativeInteger(
        double value,
        string parameterName,
        long maximum)
    {
        if (!double.IsFinite(value) || value < 0 || value > maximum || Math.Truncate(value) != value)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "Node filesystem options require a finite non-negative integer in range.");
        }
        return checked((long)value);
    }
}
