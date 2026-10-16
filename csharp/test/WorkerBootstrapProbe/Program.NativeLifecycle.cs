using System.Net;
using System.Net.Sockets;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Node;
using Tsonic.CSharp.Runtime;

internal static partial class Program
{
    private static Task<int> PhysicalJoin(Worker worker) => (Task<int>)(typeof(Worker)
        .GetField("_reader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.GetValue(worker) ?? throw new InvalidOperationException("one native physical reader Task"));

    private static int RetainedFrameCount(Worker worker)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var sync = typeof(Worker).GetField("_stateLock", flags)!.GetValue(worker)!;
        lock (sync)
            return ((System.Collections.ICollection)typeof(Worker).GetField("_signals", flags)!.GetValue(worker)!).Count;
    }

    private static Worker ConstructNativeControl(string entry, WorkerOptions options)
    {
        if (!_baseConstructionOwner) return new Worker(entry, options);
        Worker? retained = null;
        JsEventLoop.Run(() =>
        {
            retained = new Worker(entry, options);
            retained.unref();
        });
        retained!.@ref();
        return retained;
    }

    private static void VerifyNativePhysicalObservation()
    {
        RequireNoLoop();
        if (_selectedControl is null or "construction" or "cancellation" or "concurrent" or "signal-status" or "ready-cost")
        using (var listener = new TcpListener(IPAddress.Loopback, 0))
        {
            listener.Start(1);
            using var worker = ConstructNativeControl("native-worker-pending", new WorkerOptions {
                argv = new JSArray<string> { ((IPEndPoint)listener.LocalEndpoint).Port.ToString() },
            });
            using var control = listener.AcceptTcpClient();
            Require(control.GetStream().ReadByte() == 1, "native child running before standalone observation");
            RequireNoLoop();
            var called = false;
            try
            {
                worker.on<TsValue>("message", _ => called = true);
                throw new InvalidOperationException("callback admitted without owner");
            }
            catch (InvalidOperationException error) when (error.Message == "No JavaScript event loop is running on this thread.") { }
            Require(!called && worker.listenerCount("message") == 0, "failed admission leaves no callback");
            if (Selected("cancellation"))
            {
                using var cancelled = new CancellationTokenSource();
                cancelled.Cancel();
                try
                {
                    PhysicalJoin(worker).WaitAsync(cancelled.Token).GetAwaiter().GetResult();
                    throw new InvalidOperationException("native observation cancellation was lost");
                }
                catch (OperationCanceledException) { }
            }
            var first = worker.terminate();
            var second = Selected("concurrent") ? worker.terminate() : first;
            Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            Require(first.Result == second.Result, "native observers share physical status");
            if (!OperatingSystem.IsWindows() && Selected("signal-status")) Require(first.Result == 137, "standalone native SIGKILL status");
            Require(worker.terminate().GetAwaiter().GetResult() == first.Result, "repeat native observation");
            if (Selected("ready-cost"))
            {
                for (var warmup = 0; warmup < 64; warmup++) _ = worker.terminate().GetAwaiter().GetResult();
                var native = PhysicalJoin(worker);
                var baselineBefore = GC.GetTotalAllocatedBytes(precise: true);
                for (var index = 0; index < 1024; index++)
                    Require(native.GetAwaiter().GetResult() == first.Result, "handwritten ready native Task baseline");
                var baselineBytes = GC.GetTotalAllocatedBytes(precise: true) - baselineBefore;
                var before = GC.GetAllocatedBytesForCurrentThread();
                var allThreadsBefore = GC.GetTotalAllocatedBytes(precise: true);
                for (var index = 0; index < 1024; index++)
                    Require(worker.terminate().GetAwaiter().GetResult() == first.Result, "ready native completion is inline");
                Require(GC.GetAllocatedBytesForCurrentThread() - before == 0,
                    "ready native completion adds no Task wrapper or allocation");
                Require(GC.GetTotalAllocatedBytes(precise: true) - allThreadsBefore == baselineBytes && baselineBytes == 0,
                    "all-thread ready completion allocation equals the zero-allocation native Task baseline");
            }
            RequireNoLoop();
        }
        if (Selected("retained-prefix"))
        using (var listener = new TcpListener(IPAddress.Loopback, 0))
        {
            listener.Start(1);
            using var worker = ConstructNativeControl("native-worker-burst", new WorkerOptions {
                argv = new JSArray<string> { ((IPEndPoint)listener.LocalEndpoint).Port.ToString() },
            });
            using var control = listener.AcceptTcpClient();
            Require(control.GetStream().ReadByte() == 1, "native burst begins without an owner");
            Require(SpinWait.SpinUntil(() => RetainedFrameCount(worker) == 1 << 16, TimeSpan.FromSeconds(10)),
                "without owner or native observer physical input retains the full bounded prefix");
            Require(!PhysicalJoin(worker).IsCompleted, "full retained input applies honest native backpressure");
            var count = 0;
            var exits = 0;
            var owner = Environment.CurrentManagedThreadId;
            JsEventLoop.Run(() =>
            {
                worker.on<TsValue>("message", value =>
                {
                    Require(Environment.CurrentManagedThreadId == owner, "native burst callback owner thread");
                    Require(ExactInt64(value) == count, "retained physical FIFO without loss or replay");
                    count++;
                });
                worker.on<int>("exit", code =>
                {
                    Require(code == 0 && count == (1 << 17) + 17, "more than twice capacity precedes exit");
                    exits++;
                });
            });
            Require(count == (1 << 17) + 17 && exits == 1, "complete bounded physical burst");
            RequireNoLoop();
        }
        foreach (var retireOwner in new[] { false, true })
        {
            if (!Selected(retireOwner ? "retired-errors" : "unobserved-errors")) continue;
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start(1);
            var worker = ConstructNativeControl("native-worker-errors", new WorkerOptions {
                argv = new JSArray<string> { ((IPEndPoint)listener.LocalEndpoint).Port.ToString() },
            });
            try
            {
                using var control = listener.AcceptTcpClient();
                Require(control.GetStream().ReadByte() == 1, "native error prefix written");
                if (_selectedControl == "unobserved-errors")
                {
                    try
                    {
                        worker.terminate().WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
                        throw new InvalidOperationException("public native observation hid its first unobserved error");
                    }
                    catch (InvalidOperationException error) when (error.Message == "first native error")
                    {
                        Require(error.Data["AdditionalUnobservedWorkerErrors"] is ulong count && count == 1,
                            "public native observation retains the exact first error and native count");
                    }
                    RequireNoLoop();
                    continue;
                }
                var physical = PhysicalJoin(worker);
                Require(physical.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult() == 0,
                    "private physical join alone does not consume or fail on unobserved frames");
                if (retireOwner)
                {
                    var retire = new InvalidOperationException("explicit callback-owner retirement control");
                    var callbacks = 0;
                    try
                    {
                        JsEventLoop.Run(() =>
                        {
                            worker.on<Exception>("error", _ => callbacks++);
                            worker.unref();
                            var observed = worker.terminate();
                            Require(observed.IsCompletedSuccessfully && observed.Result == 0,
                                "owner-bound observation returns physical status without consuming callback errors");
                            throw retire;
                        });
                    }
                    catch (InvalidOperationException error) when (ReferenceEquals(error, retire)) { }
                    Require(callbacks == 0 && worker.listenerCount("error") == 0, "retired jobs did not pop frames");
                }
                try
                {
                    worker.terminate().GetAwaiter().GetResult();
                    throw new InvalidOperationException("native unobserved errors were hidden");
                }
                catch (InvalidOperationException error) when (error.Message == "first native error")
                {
                    Require(error.Data["AdditionalUnobservedWorkerErrors"] is ulong count && count == 1,
                        "first native error preserved and later error counted");
                }
                Require(physical.IsCompletedSuccessfully && physical.Result == 0, "physical Task never retroactively faults");
                RequireNoLoop();
            }
            finally
            {
                try { worker.Dispose(); }
                catch (InvalidOperationException error) when (error.Message == "first native error") { }
            }
        }
        if (Selected("saturation-private"))
        using (var listener = new TcpListener(IPAddress.Loopback, 0))
        {
            listener.Start(1);
            var worker = ConstructNativeControl("native-worker-burst-errors", new WorkerOptions {
                argv = new JSArray<string> { ((IPEndPoint)listener.LocalEndpoint).Port.ToString() },
            });
            try
            {
                using var control = listener.AcceptTcpClient();
                Require(control.GetStream().ReadByte() == 1, "native saturation producer started");
                Require(SpinWait.SpinUntil(() => RetainedFrameCount(worker) == 1 << 16, TimeSpan.FromSeconds(10)),
                    "native observation begins from actual bounded saturation");
                var observe = typeof(Worker).GetMethod("ObserveCompletion",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?? throw new InvalidOperationException("canonical native observation owner");
                var completion = (Task<int>)observe.Invoke(worker, null)!;
                try
                {
                    completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
                    throw new InvalidOperationException("native saturation lost its errors");
                }
                catch (InvalidOperationException error) when (error.Message == "native error at 0")
                {
                    Require(error.Data["AdditionalUnobservedWorkerErrors"] is ulong count && count == 2,
                        "native saturation retains errors before, at and beyond capacity");
                }
                Require(PhysicalJoin(worker).IsCompletedSuccessfully && PhysicalJoin(worker).Result == 0,
                    "native saturation error reported after actual cleanup and join");
                RequireNoLoop();
            }
            finally
            {
                try { worker.Dispose(); }
                catch (InvalidOperationException error) when (error.Message == "native error at 0") { }
            }
        }
        if (Selected("default-ref"))
        {
        Worker? zeroListeners = null;
        try
        {
            JsEventLoop.Run(() => zeroListeners = new Worker("native-worker-empty"));
            Require(PhysicalJoin(zeroListeners!).WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult() == 0,
                "default-ref with zero listeners keeps delivery alive through native physical completion");
        }
        finally { zeroListeners?.Dispose(); }
        }
        VerifyPublicSaturatedTerminationAndDisposal();
        if (Selected("rebind")) VerifyRetiredWorkerRebindsRemainingFifo();
        if (Selected("owner-saturated-dispose")) VerifyOwnerThreadSaturatedDispose();
        if (Selected("io-before-exit")) VerifyNaturalNativeIoObservation(false);
        if (Selected("io-after-exit")) VerifyNaturalNativeIoObservation(true);
    }

    private static void VerifyOwnerThreadSaturatedDispose()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(1);
        Worker? worker = null;
        var callbacks = 0;
        JsEventLoop.Run(() =>
        {
            worker = new Worker("native-worker-saturated", new WorkerOptions {
                argv = new JSArray<string> { ((IPEndPoint)listener.LocalEndpoint).Port.ToString() },
            });
            worker.on<TsValue>("message", _ => callbacks++);
            worker.on<Exception>("error", _ => callbacks++);
            worker.on<int>("exit", _ => callbacks++);
            using var control = listener.AcceptTcpClient();
            Require(control.GetStream().ReadByte() == 1, "owner-thread native saturation producer started");
            Require(SpinWait.SpinUntil(() => RetainedFrameCount(worker) == 1 << 16, TimeSpan.FromSeconds(10)),
                "owner-thread Dispose starts with a full FIFO and uninvoked delivery");
            if (_selectedControl is not null) Console.WriteLine("P05 owner-saturated-dispose: full native FIFO; entering public Dispose");
            try
            {
                worker.Dispose();
                throw new InvalidOperationException("owner-thread Dispose hid its first unobserved error");
            }
            catch (InvalidOperationException error) when (error.Message == "public saturated native error") { }
            Require(PhysicalJoin(worker).IsCompletedSuccessfully, "owner-thread Dispose retires before its finite native cleanup wait");
            Require(callbacks == 0, "Dispose does not secretly pump or offload callback delivery");
            Require(worker.listenerCount("message") == 0 && worker.listenerCount("error") == 0 && worker.listenerCount("exit") == 0,
                "Dispose retires only this worker's callback ownership");
            Require(!ProcessKeepAlive.HasReferences, "worker native reference is released before leaving the active owner");
        });
        Require(callbacks == 0 && RetainedFrameCount(worker!) == 0, "no later old-owner job replays disposed physical input");
        RequireNoLoop();
    }

    private static void VerifyNaturalNativeIoObservation(bool observeAfterExit)
    {
        RequireNoLoop();
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(1);
        var worker = ConstructNativeControl("native-worker-live-failure", new WorkerOptions {
            argv = new JSArray<string> { ((IPEndPoint)listener.LocalEndpoint).Port.ToString() },
        });
        try
        {
            using var control = listener.AcceptTcpClient();
            Require(control.GetStream().ReadByte() == 1, "actual truncated transport reached its live-process boundary");
            var observe = typeof(Worker).GetMethod("ObserveCompletion",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?? throw new InvalidOperationException("one canonical explicit native observation owner");
            if (observeAfterExit)
            {
                control.GetStream().WriteByte(1);
                Require(PhysicalJoin(worker).WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult() == 0,
                    "actual process exit is observed before the native consumer");
                var completion = (Task<int>)observe.Invoke(worker, null)!;
                Require(completion.GetAwaiter().GetResult() == 0, "IO consumed after observed exit is suppressed without a timer");
            }
            else
            {
                var completion = (Task<int>)observe.Invoke(worker, null)!;
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var sync = typeof(Worker).GetField("_stateLock", flags)!.GetValue(worker)!;
                Require(SpinWait.SpinUntil(() => {
                    lock (sync) return (ulong)typeof(Worker).GetField("_nativeFailureCount", flags)!.GetValue(worker)! == 1;
                }, TimeSpan.FromSeconds(10)), "pre-exit native IO consumption is counted before releasing the live child");
                Require(!PhysicalJoin(worker).IsCompleted, "later process exit cannot explain earlier consumption");
                control.GetStream().WriteByte(1);
                try
                {
                    completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
                    throw new InvalidOperationException("natural native completion erased its previously consumed IO error");
                }
                catch (EndOfStreamException) { }
                Require(PhysicalJoin(worker).IsCompletedSuccessfully && PhysicalJoin(worker).Result == 0,
                    "later normal physical status does not retroactively alter the native observation's error");
            }
        }
        finally
        {
            try { worker.Dispose(); }
            catch (EndOfStreamException) when (!observeAfterExit) { }
        }
    }

    private static void VerifyPublicSaturatedTerminationAndDisposal()
    {
        foreach (var dispose in new[] { false, true })
        {
            if (!Selected(dispose ? "saturated-dispose" : "saturated-terminate")) continue;
            RequireNoLoop();
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start(1);
            var worker = ConstructNativeControl("native-worker-saturated", new WorkerOptions {
                argv = new JSArray<string> { ((IPEndPoint)listener.LocalEndpoint).Port.ToString() },
            });
            try
            {
                using var control = listener.AcceptTcpClient();
                Require(control.GetStream().ReadByte() == 1, "public saturation producer began");
                Require(SpinWait.SpinUntil(() => RetainedFrameCount(worker) == 1 << 16, TimeSpan.FromSeconds(10)),
                    "public native operation begins with an actually full physical FIFO");
                Require(!PhysicalJoin(worker).IsCompleted, "bounded backpressure prevents pretend physical completion");
                try
                {
                    if (dispose) worker.Dispose();
                    else worker.terminate().WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
                    throw new InvalidOperationException("public native observation lost its retained error");
                }
                catch (InvalidOperationException error) when (error.Message == "public saturated native error") { }
                var joined = PhysicalJoin(worker);
                Require(joined.IsCompletedSuccessfully, "public terminate or Dispose drains and joins the actual reader");
                if (!OperatingSystem.IsWindows()) Require(joined.Result == 137, "public saturated native SIGKILL status");
                Require(RetainedFrameCount(worker) == 0, "public native observation consumes the entire retained prefix");
                Require(worker.listenerCount("message") == 0 && worker.listenerCount("error") == 0,
                    "native progress never invents callback ownership");
                RequireNoLoop();
            }
            finally
            {
                try { worker.Dispose(); }
                catch (InvalidOperationException error) when (error.Message == "public saturated native error") { }
            }
        }
    }

    private static void VerifyRetiredWorkerRebindsRemainingFifo()
    {
        RequireNoLoop();
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(1);
        using var worker = ConstructNativeControl("native-worker-burst", new WorkerOptions {
            argv = new JSArray<string> { ((IPEndPoint)listener.LocalEndpoint).Port.ToString() },
        });
        using var control = listener.AcceptTcpClient();
        Require(control.GetStream().ReadByte() == 1, "rebind native producer started");
        Require(SpinWait.SpinUntil(() => RetainedFrameCount(worker) == 1 << 16, TimeSpan.FromSeconds(10)),
            "old callback owner starts with actual bounded pressure");
        var retire = new InvalidOperationException("midstream callback owner retirement");
        var count = 0;
        var oldExits = 0;
        try
        {
            JsEventLoop.Run(() =>
            {
                worker.on<TsValue>("message", value =>
                {
                    Require(ExactInt64(value) == count, "old owner delivers only its exact prefix");
                    count++;
                    if (count == 257) throw retire;
                });
                worker.on<int>("exit", _ => oldExits++);
            });
            throw new InvalidOperationException("old owner was not retired at the specified prefix");
        }
        catch (InvalidOperationException error) when (ReferenceEquals(error, retire)) { }
        Require(count == 257 && oldExits == 0, "retired owner never invokes its unconsumed tail or exit");
        Require(worker.listenerCount("message") == 0 && worker.listenerCount("exit") == 0,
            "retirement discards callbacks rather than physical frames");
        var exits = 0;
        JsEventLoop.Run(() =>
        {
            worker.on<TsValue>("message", value =>
            {
                Require(ExactInt64(value) == count, "new owner continues exact FIFO without loss or replay");
                count++;
            });
            worker.on<int>("exit", code =>
            {
                Require(code == 0 && count == (1 << 17) + 17, "new owner receives exit after the whole retained prefix");
                exits++;
            });
        });
        Require(count == (1 << 17) + 17 && exits == 1 && oldExits == 0, "new callback owner receives one complete remaining FIFO and exit");
        RequireNoLoop();
    }
}
