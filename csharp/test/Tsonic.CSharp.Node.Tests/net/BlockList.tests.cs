using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class BlockListTests
{
    [Fact]
    public void BlockList_Constructor_CreatesInstance()
    {
        var blockList = new BlockList();
        Assert.NotNull(blockList);
    }

    [Fact]
    public void BlockList_AddAddress_AddsToList()
    {
        var blockList = new BlockList();
        blockList.addAddress("192.168.1.1");

        Assert.True(blockList.check("192.168.1.1"));
    }

    [Fact]
    public void BlockList_AddAddress_WithType_AddsToList()
    {
        var blockList = new BlockList();
        blockList.addAddress("192.168.1.1", "ipv4");

        Assert.True(blockList.check("192.168.1.1", "ipv4"));
    }

    [Fact]
    public void BlockList_Check_NotBlocked_ReturnsFalse()
    {
        var blockList = new BlockList();
        blockList.addAddress("192.168.1.1");

        Assert.False(blockList.check("192.168.1.2"));
    }

    [Fact]
    public void BlockList_AddRange_BlocksRange()
    {
        var blockList = new BlockList();
        blockList.addRange("192.168.1.0", "192.168.1.255", "ipv4");

        Assert.True(blockList.check("192.168.1.100", "ipv4"));
        Assert.True(blockList.check("192.168.1.1", "ipv4"));
        Assert.True(blockList.check("192.168.1.255", "ipv4"));
        Assert.False(blockList.check("192.168.2.1", "ipv4"));
    }

    [Fact]
    public void BlockList_AddSubnet_BlocksSubnet()
    {
        var blockList = new BlockList();
        blockList.addSubnet("192.168.1.0", 24, "ipv4");

        Assert.True(blockList.check("192.168.1.100", "ipv4"));
        Assert.True(blockList.check("192.168.1.1", "ipv4"));
        Assert.False(blockList.check("192.168.2.1", "ipv4"));
    }

    [Fact]
    public void BlockList_GetRules_ReturnsAllRules()
    {
        var blockList = new BlockList();
        blockList.addAddress("192.168.1.1");
        blockList.addRange("10.0.0.0", "10.0.0.255", "ipv4");
        blockList.addSubnet("172.16.0.0", 16, "ipv4");

        var rules = blockList.getRules();
        Assert.NotEmpty(rules);
        Assert.True(rules.Length >= 3);
    }
}
