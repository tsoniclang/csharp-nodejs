using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class GapGuardTests
{
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
