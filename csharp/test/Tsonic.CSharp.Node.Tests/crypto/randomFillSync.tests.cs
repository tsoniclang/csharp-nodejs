using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class randomFillSyncTests
{
    [Fact]
    public void randomFillSync_FillsBuffer()
    {
        var buffer = new byte[32];
        crypto.randomFillSync(buffer);

        // Check that buffer is not all zeros
        var allZeros = true;
        foreach (var b in buffer)
        {
            if (b != 0)
            {
                allZeros = false;
                break;
            }
        }
        Assert.False(allZeros);
    }

    [Fact]
    public void randomFillSync_WithOffsetAndSize()
    {
        var buffer = new byte[64];
        crypto.randomFillSync(buffer, 16, 32);

        // Check first 16 bytes are still zero
        for (int i = 0; i < 16; i++)
        {
            Assert.Equal(0, buffer[i]);
        }

        // Check that middle 32 bytes have random data
        var hasNonZero = false;
        for (int i = 16; i < 48; i++)
        {
            if (buffer[i] != 0)
            {
                hasNonZero = true;
                break;
            }
        }
        Assert.True(hasNonZero);
    }
}
