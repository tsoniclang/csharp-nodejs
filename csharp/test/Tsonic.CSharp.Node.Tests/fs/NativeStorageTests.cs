using System.Text;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class NativeStorageTests : FsTestBase
{
    [Theory]
    [InlineData(0)]
    [InlineData(64)]
    [InlineData(1024)]
    public void SmallTextReadsStayWithinNativeAllocationCost(int length)
    {
        var path = GetTestPath("small.txt");
        var text = new string('x', length);
        var encoding = new UTF8Encoding(false);
        File.WriteAllText(path, text, encoding);
        Assert.Equal(text, fs.readFileSync(path, "utf8"));
        Assert.Equal(text, File.ReadAllText(path, encoding));
        var before = GC.GetAllocatedBytesForCurrentThread();
        var actual = fs.readFileSync(path, "utf8");
        var runtimeBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        before = GC.GetAllocatedBytesForCurrentThread();
        var expected = File.ReadAllText(path, encoding);
        var nativeBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(expected, actual);
        Assert.True(runtimeBytes <= nativeBytes, $"Runtime {runtimeBytes} bytes exceeds native {nativeBytes}");
    }

    [Theory]
    [InlineData("utf8")]
    [InlineData("ascii")]
    [InlineData("utf16")]
    [InlineData("utf32")]
    public async Task TextOperationsMatchNativeEncodingAndPreserveBoundaries(string encoding)
    {
        var codec = encoding switch
        {
            "ascii" => Encoding.ASCII,
            "utf16" => Encoding.Unicode,
            "utf32" => Encoding.UTF32,
            _ => new UTF8Encoding(false)
        };
        var path = GetTestPath("text.txt");
        var expected = GetTestPath("expected.txt");
        foreach (var text in new[] { "", "café😀", new string('x', 65535) + "😀" + new string('y', 70000), "\ud800last" })
        {
            File.WriteAllText(expected, text, codec);
            fs.writeFileSync(path, text, encoding);
            Assert.Equal(File.ReadAllBytes(expected), File.ReadAllBytes(path));
            Assert.Equal(File.ReadAllText(expected, codec), fs.readFileSync(path, encoding));
            await fs.writeFile(path, text, encoding);
            Assert.Equal(File.ReadAllBytes(expected), File.ReadAllBytes(path));
            Assert.Equal(File.ReadAllText(expected, codec), await fs.readFile(path, encoding));
        }
        File.WriteAllText(path, "bom😀", Encoding.Unicode);
        Assert.Equal("bom😀", fs.readFileSync(path, "utf8"));
        Assert.Equal("bom😀", await fs.readFile(path, "utf8"));
    }

    [Fact]
    public async Task MetadataSnapshotsResolveLinksWithoutCrossCallCaching()
    {
        var file = GetTestPath("target.txt");
        var link = GetTestPath("link.txt");
        File.WriteAllText(file, "first", new UTF8Encoding(false));
        File.CreateSymbolicLink(link, file);
        Assert.True(fs.lstatSync(link).isSymbolicLink);
        Assert.Equal(5, fs.statSync(link).size);
        Assert.False(fs.statSync(link).isSymbolicLink);
        File.AppendAllText(file, "second");
        Assert.Equal(11, (await fs.stat(link)).size);
        var directory = GetTestPath("directory");
        var directoryLink = GetTestPath("directory-link");
        Directory.CreateDirectory(directory);
        Directory.CreateSymbolicLink(directoryLink, directory);
        Assert.True(fs.statSync(directoryLink).isDirectory);
        Assert.True(fs.lstatSync(directoryLink).isSymbolicLink);
        File.Delete(file);
        Assert.Throws<FileNotFoundException>(() => fs.statSync(link));
        Assert.True(fs.lstatSync(link).isSymbolicLink);
    }
}
