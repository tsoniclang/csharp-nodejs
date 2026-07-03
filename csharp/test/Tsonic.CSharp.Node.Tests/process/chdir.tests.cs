using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class chdirTests : IDisposable
{
    private readonly string _originalDirectory;
    private readonly string _tempDirectory;

    public chdirTests()
    {
        _originalDirectory = Directory.GetCurrentDirectory();
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"tsonic-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        // Restore original directory
        Directory.SetCurrentDirectory(_originalDirectory);

        // Clean up temp directory
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void chdir_ShouldChangeCurrentDirectory()
    {
        process.chdir(_tempDirectory);

        var cwd = process.cwd();
        Assert.Equal(_tempDirectory, cwd);
    }

    [Fact]
    public void chdir_ShouldUpdateDirectoryGetCurrentDirectory()
    {
        process.chdir(_tempDirectory);

        var current = Directory.GetCurrentDirectory();
        Assert.Equal(_tempDirectory, current);
    }

    [Fact]
    public void chdir_ShouldThrowForNullDirectory()
    {
        Assert.Throws<ArgumentException>(() => process.chdir(null!));
    }

    [Fact]
    public void chdir_ShouldThrowForEmptyDirectory()
    {
        Assert.Throws<ArgumentException>(() => process.chdir(string.Empty));
    }

    [Fact]
    public void chdir_ShouldThrowForNonExistentDirectory()
    {
        var nonExistent = Path.Combine(_tempDirectory, "does-not-exist");

        Assert.Throws<DirectoryNotFoundException>(() => process.chdir(nonExistent));
    }

    [Fact]
    public void chdir_ShouldWorkWithRelativePaths()
    {
        process.chdir(_tempDirectory);

        var subDir = "subdir";
        var subDirPath = Path.Combine(_tempDirectory, subDir);
        Directory.CreateDirectory(subDirPath);

        process.chdir(subDir);

        var cwd = process.cwd();
        Assert.Equal(subDirPath, cwd);
    }
}
