using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public sealed class WorkerBootstrapProcessTests
{
    [Fact]
    public void NativeBrokenPipeRetainsItsSocketErrorWithoutAcknowledgmentIo()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(1);
        using var sender = new TcpClient(AddressFamily.InterNetwork);
        sender.Connect((IPEndPoint)listener.LocalEndpoint);
        using var receiver = listener.AcceptTcpClient();
        using var transport = new WorkerTransport(sender);
        sender.Client.Shutdown(SocketShutdown.Send);
        var error = Assert.Throws<IOException>(() => transport.Send(WorkerFrameKind.Message, [1]));
        var native = Assert.IsType<SocketException>(error.InnerException);
        Assert.Equal(SocketError.Shutdown, native.SocketErrorCode);
        Assert.True(WorkerTransport.IsBrokenPipe(error), "only proven native EPIPE is counted");
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(5, 2)]
    public void NativeFrameFailuresRetainTheExactKindAndFramePhase(int prefix, int expected)
    {
        using var stream = new FailingStream(prefix);
        var phase = WorkerFramePhase.Boundary;
        var error = Assert.Throws<IOException>(() => WorkerTransport.ReadFrame(stream, out phase));
        Assert.True(ReferenceEquals(stream.Failure, error), "the original native IO exception is retained");
        Assert.Equal(SocketError.ConnectionReset, WorkerTransport.NativeSocketError(error));
        Assert.Equal((WorkerFramePhase)expected, phase);
    }

    private sealed class FailingStream(int prefix) : MemoryStream(new byte[] { 4, 0, 0, 0, 1 }, 0, prefix)
    {
        internal IOException Failure { get; } = new("native reset", new SocketException((int)SocketError.ConnectionReset));
        public override int Read(Span<byte> buffer)
        {
            if (Position == Length) throw Failure;
            return base.Read(buffer);
        }
    }

    [Fact]
    public void NativeFrameEndRetainsCompleteMessagesWithoutManufacturingAnError()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(1);
        using var sender = new TcpClient(AddressFamily.InterNetwork);
        sender.Connect((IPEndPoint)listener.LocalEndpoint);
        using var receiver = listener.AcceptTcpClient();
        receiver.ReceiveTimeout = 5000;
        using var transport = new WorkerTransport(receiver);
        using var peer = new WorkerTransport(sender);
        for (var index = 0; index < 64; index++)
            peer.Send(WorkerFrameKind.Message, [(byte)index]);
        sender.Client.Shutdown(SocketShutdown.Send);
        for (var index = 0; index < 64; index++)
        {
            var frame = Assert.IsType<WorkerFrame>(transport.Receive());
            Assert.Equal(WorkerFrameKind.Message, frame.Kind);
            Assert.Equal(new byte[] { (byte)index }, frame.Payload);
        }
        object? end = transport.Receive();
        Assert.True(end is null, "clean native EOF is an absent frame");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void NativeFrameTruncationRemainsAnIoFailure(int length)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(1);
        using var sender = new TcpClient(AddressFamily.InterNetwork);
        sender.Connect((IPEndPoint)listener.LocalEndpoint);
        using var receiver = listener.AcceptTcpClient();
        receiver.ReceiveTimeout = 5000;
        using var transport = new WorkerTransport(receiver);
        byte[] frame = [(byte)WorkerFrameKind.Message, 0, 0, 0, 2, 7];
        sender.GetStream().Write(frame.AsSpan(0, length));
        sender.Client.Shutdown(SocketShutdown.Send);
        Assert.Throws<EndOfStreamException>(() => transport.Receive());
        Assert.Equal(length < 5 ? WorkerFramePhase.Header : WorkerFramePhase.Payload, transport.ReadPhase);
    }

    [Fact]
    public async Task PublicNativeBootstrapWorksWithoutAnImplicitLoopAndRetiresOnlyCallbacks()
    {
        var relativePath = typeof(WorkerBootstrapProcessTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "WorkerBootstrapProbePath").Value;
        Assert.False(string.IsNullOrEmpty(relativePath));
        var probePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath));
        Assert.True(File.Exists(probePath), "ordinary native build produced bootstrap fixture");
        Assert.True(File.Exists(Path.ChangeExtension(probePath, ".runtimeconfig.json")), "probe runtime configuration");
        Assert.True(File.Exists(Path.ChangeExtension(probePath, ".deps.json")), "probe dependency manifest");
        var start = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(probePath),
        };
        start.ArgumentList.Add(probePath);
        using var child = new System.Diagnostics.Process { StartInfo = start };
        Assert.True(child.Start(), "native bootstrap fixture started");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            var output = ReadBounded(child.StandardOutput, child, timeout.Token);
            var error = ReadBounded(child.StandardError, child, timeout.Token);
            await child.WaitForExitAsync(timeout.Token);
            var standardOutput = await output;
            var standardError = await error;
            Assert.True(child.ExitCode == 0, $"native bootstrap exit {child.ExitCode}: {standardError}");
            Assert.Equal(string.Empty, standardError);
            using var result = JsonDocument.Parse(standardOutput);
            Assert.Equal(5, result.RootElement.EnumerateObject().Count());
            Assert.True(result.RootElement.GetProperty("public_native_bootstrap").GetBoolean());
            Assert.True(result.RootElement.GetProperty("exact_int64").GetBoolean());
            Assert.True(result.RootElement.GetProperty("preloop_transport").GetBoolean());
            Assert.True(result.RootElement.GetProperty("retained_transport").GetBoolean());
            Assert.True(result.RootElement.GetProperty("fresh_epoch").GetBoolean());
        }
        finally
        {
            if (!child.HasExited)
            {
                try { child.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) when (child.HasExited) { }
                await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            }
        }
    }

    private static async Task<string> ReadBounded(StreamReader reader, System.Diagnostics.Process child,
        CancellationToken cancellation)
    {
        var collected = new StringBuilder();
        var buffer = new char[1024];
        while (true)
        {
            var count = await reader.ReadAsync(buffer.AsMemory(), cancellation);
            if (count == 0) return collected.ToString();
            if (collected.Length + count > 16384)
            {
                if (!child.HasExited)
                {
                    try { child.Kill(entireProcessTree: true); }
                    catch (InvalidOperationException) when (child.HasExited) { }
                }
                throw new InvalidOperationException("Native bootstrap fixture exceeded its finite output limit.");
            }
            collected.Append(buffer, 0, count);
        }
    }

    [Theory]
    [InlineData(5, 2)]
    [InlineData(2, 4)]
    public void FullNativeFrameUsesTheEquivalentExactReadCount(int chunk, int expected)
    {
        byte[] bytes = [(byte)WorkerFrameKind.Message, 0, 0, 0, 1, 17];
        using var stream = new CountedStream(bytes, chunk);
        var frame = Assert.IsType<WorkerFrame>(WorkerTransport.ReadFrame(stream, out _));
        Assert.Equal(WorkerFrameKind.Message, frame.Kind);
        Assert.Equal(new byte[] { 17 }, frame.Payload);
        Assert.Equal(expected, stream.Reads);
        using var baseline = new CountedStream(bytes, chunk);
        Span<byte> header = stackalloc byte[5];
        Span<byte> payload = stackalloc byte[1];
        baseline.ReadExactly(header);
        baseline.ReadExactly(payload);
        Assert.Equal(baseline.Reads, stream.Reads);
    }

    private sealed class CountedStream(byte[] bytes, int chunk) : MemoryStream(bytes)
    {
        internal int Reads { get; private set; }

        public override int Read(Span<byte> buffer)
        {
            Reads++;
            return base.Read(buffer[..Math.Min(buffer.Length, chunk)]);
        }
    }

    [Fact]
    public void NativeWorkerSocketDoesNotLeakIntoAnExecutedDescendant()
    {
        if (!OperatingSystem.IsLinux()) return;
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(1);
        using var sender = new TcpClient(AddressFamily.InterNetwork);
        sender.Connect((IPEndPoint)listener.LocalEndpoint);
        using var receiver = listener.AcceptTcpClient();
        foreach (var socket in new[] { sender.Client, receiver.Client })
        {
            var flags = File.ReadLines($"/proc/self/fdinfo/{socket.Handle.ToInt64()}")
                .Single(line => line.StartsWith("flags:", StringComparison.Ordinal));
            var nativeFlags = Convert.ToInt64(flags["flags:".Length..].Trim(), 8);
            Assert.True((nativeFlags & 0x80000) != 0, "native worker socket carries close-on-exec");
        }
    }
}
