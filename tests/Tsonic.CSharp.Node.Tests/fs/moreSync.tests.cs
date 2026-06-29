using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class FsMoreSyncTests : FsTestBase
{
    [Fact]
    public void Constants_ExposeAccessAndModeFlags()
    {
        Assert.Equal(0, fs.constants.F_OK);
        Assert.True(fs.constants.R_OK > 0);
        Assert.True(fs.constants.S_IFREG > 0);
    }

    [Fact]
    public void LstatAndStatfs_ReturnClosedCarriers()
    {
        var path = GetTestPath("file.txt");
        File.WriteAllText(path, "hello");

        var stat = fs.lstatSync(path);
        var statfs = fs.statfsSync(path);

        Assert.True(stat.isFile);
        Assert.True(stat.nlink >= 1);
        Assert.True(statfs.bsize > 0);
        Assert.True(statfs.blocks > 0);
    }

    [Fact]
    public void ReaddirDirentsAndOpendir_ReturnDirectoryEntries()
    {
        var child = GetTestPath("child.txt");
        File.WriteAllText(child, "hello");

        var dirents = fs.readdirDirentsSync(_testDir);
        var dir = fs.opendirSync(_testDir);

        Assert.Contains(dirents, entry => entry.name == "child.txt" && entry.isFile());
        Assert.NotNull(dir.read());
        dir.close();
        Assert.True(dir.closed);
    }

    [Fact]
    public void VectorIo_ReadsAndWritesMultipleBuffers()
    {
        var path = GetTestPath("vector.txt");
        var fd = fs.openSync(path, "w+");
        try
        {
            var write = fs.writevSync(fd, [Encoding.UTF8.GetBytes("ab"), Encoding.UTF8.GetBytes("cd")], 0);
            var left = new byte[2];
            var right = new byte[2];
            var read = fs.readvSync(fd, [left, right], 0);

            Assert.Equal(4, write.bytesWritten);
            Assert.Equal(4, read.bytesRead);
            Assert.Equal("ab", Encoding.UTF8.GetString(left));
            Assert.Equal("cd", Encoding.UTF8.GetString(right));
        }
        finally
        {
            fs.closeSync(fd);
        }
    }

    [Fact]
    public void MkdtempDisposable_RemovesDirectory()
    {
        var temp = fs.mkdtempDisposableSync(Path.Combine(_testDir, "tmp-"));

        Assert.True(Directory.Exists(temp.path));
        temp.remove();

        Assert.True(temp.removed);
        Assert.False(Directory.Exists(temp.path));
    }

    [Fact]
    public void CreateReadStream_ReadsFileIntoReadable()
    {
        var path = GetTestPath("stream.txt");
        File.WriteAllText(path, "stream");

        var stream = fs.createReadStream(path);
        var bytes = Assert.IsType<byte[]>(stream.read());

        Assert.Equal("stream", Encoding.UTF8.GetString(bytes));
        Assert.Equal(6, stream.bytesRead);
    }

    [Fact]
    public async Task FileHandle_ReadWriteAndStat_Work()
    {
        var path = GetTestPath("handle.txt");
        var handle = await fs_promises.open(path, "w+");
        try
        {
            await handle.write("hello", 0);
            var buffer = new byte[5];
            await handle.read(buffer, 0, 5, 0);
            var stat = await handle.stat();

            Assert.Equal("hello", Encoding.UTF8.GetString(buffer));
            Assert.True(stat.isFile);
        }
        finally
        {
            await handle.close();
        }
    }
}
