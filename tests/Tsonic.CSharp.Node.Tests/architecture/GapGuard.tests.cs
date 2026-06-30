using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class GapGuardTests
{
    private const int MaximumSourceFileLines = 500;

    [Fact]
    public void Source_ShouldNotExposeNotImplementedRuntimePaths()
    {
        var root = FindRepositoryRoot();
        var sourceFiles = Directory.GetFiles(Path.Combine(root, "src", "Tsonic.CSharp.Node"), "*.cs", SearchOption.AllDirectories);

        foreach (var sourceFile in sourceFiles)
        {
            var text = File.ReadAllText(sourceFile);
            Assert.DoesNotContain("NotImplementedException", text);
            Assert.DoesNotContain("not yet implemented", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("stub implementation", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Zstd_ShouldFailClosedInsteadOfUsingAnotherCodec()
    {
        var root = FindRepositoryRoot();
        var sourceFile = Path.Combine(root, "src", "Tsonic.CSharp.Node", "zlib", "extended.cs");
        var text = File.ReadAllText(sourceFile);

        Assert.DoesNotContain("zstdCompressSync(byte[] buffer, ZstdOptions? options = null) { _ = options; return brotliCompressSync(buffer); }", text);
        Assert.DoesNotContain("zstdDecompressSync(byte[] buffer, ZstdOptions? options = null) { _ = options; return brotliDecompressSync(buffer); }", text);
    }

    [Fact]
    public void SourceAndTests_ShouldKeepFilesSemanticallySplit()
    {
        var root = FindRepositoryRoot();
        var files = Directory
            .GetFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(Path.Combine(root, "tests"), "*.cs", SearchOption.AllDirectories));

        var oversizedFiles = files
            .Select(file => new
            {
                Path = Path.GetRelativePath(root, file),
                Lines = File.ReadLines(file).Count(),
            })
            .Where(file => file.Lines > MaximumSourceFileLines)
            .OrderByDescending(file => file.Lines)
            .ThenBy(file => file.Path)
            .Select(file => $"{file.Lines} {file.Path}")
            .ToArray();

        Assert.True(
            oversizedFiles.Length == 0,
            $"Files over {MaximumSourceFileLines} lines must be split by semantic responsibility:{Environment.NewLine}{string.Join(Environment.NewLine, oversizedFiles)}");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src", "Tsonic.CSharp.Node")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate csharp-nodejs repository root.");
    }
}
