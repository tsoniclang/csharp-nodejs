namespace Tsonic.CSharp.Node.Tests;

public class FsTestBase : IDisposable
{
    protected readonly string _testDir;

    public FsTestBase()
    {
        // Create a unique test directory for this test run
        _testDir = Path.Combine(Path.GetTempPath(), $"csharp-nodejs-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    protected string GetTestPath(string filename) => Path.Combine(_testDir, filename);
}
