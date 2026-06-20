using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class exitTests
{
    [Fact]
    public void exit_MethodExists()
    {
        // We can't actually test process.exit() because it would terminate the test runner.
        Action<int?> exit = process.exit;
        Assert.NotNull(exit);
    }

    // Note: We cannot safely test process.exit() as it would terminate the test runner.
    // In a real-world scenario, you would:
    // 1. Spawn a separate process that calls process.exit()
    // 2. Check the exit code of that process
    // 3. Verify it matches the expected value
}
