using System;
using System.IO;
using System.Threading;
using Xunit;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node.Tests;

public class ConsoleTests
{
    [Fact]
    public void log_ShouldNotThrow()
    {
        console.log("test message");
        console.log("test with %s", "format");
        console.log(null);
    }

    [Fact]
    public void error_ShouldNotThrow()
    {
        console.error("error message");
        console.error("error with %s", "format");
        console.error(null);
    }

    [Fact]
    public void warn_ShouldNotThrow()
    {
        console.warn("warning message");
        console.warn("warning with %s", "format");
    }

    [Fact]
    public void info_ShouldNotThrow()
    {
        console.info("info message");
        console.info("info with %s", "format");
    }

    [Fact]
    public void debug_ShouldNotThrow()
    {
        console.debug("debug message");
        console.debug("debug with %s", "format");
    }

    [Fact]
    public void assert_ShouldNotThrowWhenTrue()
    {
        console.assert(true, "This should not print");
        console.assert(1 == 1, "This should not print either");
    }

    [Fact]
    public void assert_ShouldOutputWhenFalse()
    {
        // Should output to stderr but not throw
        console.assert(false, "This assertion failed");
        console.assert(1 == 2, "Math is broken");
    }

    [Fact]
    public void clear_ShouldNotThrow()
    {
        console.clear();
    }

    [Fact]
    public void count_ShouldTrackCounts()
    {
        // Reset counter first
        console.countReset("testCounter");

        console.count("testCounter");
        console.count("testCounter");
        console.count("testCounter");

        // Should have counted 3 times
        // (We can't easily verify the output, but we ensure it doesn't throw)
    }

    [Fact]
    public void count_ShouldUseDefaultLabel()
    {
        console.countReset();
        console.count();
        console.count();
        // Should count with "default" label
    }

    [Fact]
    public void countReset_ShouldResetCounter()
    {
        console.count("resetTest");
        console.count("resetTest");
        console.countReset("resetTest");
        console.count("resetTest");
        // After reset, counter should be 1
    }

    [Fact]
    public void dir_ShouldNotThrow()
    {
        console.dir(new JSObject { ["name"] = "test", ["value"] = 42 });
        console.dir("simple string");
        console.dir(null);
    }

    [Fact]
    public void dirxml_ShouldNotThrow()
    {
        console.dirxml(new JSObject { ["name"] = "test" });
        console.dirxml("test", 123, new JSObject());
    }

    [Fact]
    public void group_ShouldNotThrow()
    {
        console.group("Test Group");
        console.log("Inside group");
        console.groupEnd();
    }

    [Fact]
    public void group_WithoutLabel()
    {
        console.group();
        console.log("Inside anonymous group");
        console.groupEnd();
    }

    [Fact]
    public void groupCollapsed_ShouldNotThrow()
    {
        console.groupCollapsed("Collapsed Group");
        console.log("Inside collapsed group");
        console.groupEnd();
    }

    [Fact]
    public void nestedGroups_ShouldWork()
    {
        console.group("Outer");
        console.log("Outer message");
        console.group("Inner");
        console.log("Inner message");
        console.groupEnd();
        console.log("Back to outer");
        console.groupEnd();
    }

    [Fact]
    public void groupEnd_WithoutGroup_ShouldNotThrow()
    {
        console.groupEnd();
        console.groupEnd();
        console.groupEnd();
    }

    [Fact]
    public void table_ShouldNotThrow()
    {
        console.table(new TsArray(new object?[] {
            new JSObject { ["name"] = "Alice", ["age"] = 30 },
            new JSObject { ["name"] = "Bob", ["age"] = 25 }
        }));

        console.table(null);
        console.table("simple string");
    }

    [Fact]
    public void time_ShouldMeasureElapsedTime()
    {
        console.time("testTimer");
        Thread.Sleep(10); // Sleep for 10ms
        console.timeEnd("testTimer");
        // Should print something like "testTimer: 10ms" or similar
    }

    [Fact]
    public void time_WithDefaultLabel()
    {
        console.time();
        Thread.Sleep(5);
        console.timeEnd();
    }

    [Fact]
    public void timeLog_ShouldLogIntermediateTime()
    {
        console.time("logTimer");
        Thread.Sleep(5);
        console.timeLog("logTimer", "checkpoint 1");
        Thread.Sleep(5);
        console.timeLog("logTimer", "checkpoint 2");
        console.timeEnd("logTimer");
    }

    [Fact]
    public void timeLog_WithData()
    {
        console.time("dataTimer");
        console.timeLog("dataTimer", "value", 42, "more data");
        console.timeEnd("dataTimer");
    }

    [Fact]
    public void timeEnd_WithoutMatchingTime_ShouldNotThrow()
    {
        console.timeEnd("nonExistentTimer");
    }

    [Fact]
    public void trace_ShouldNotThrow()
    {
        console.trace("Trace message");
        console.trace("Trace with %s", "formatting");
        console.trace();
    }

    [Fact]
    public void profile_ShouldNotThrow()
    {
        console.profile("testProfile");
        console.profileEnd("testProfile");
    }

    [Fact]
    public void profileEnd_WithoutProfile_ShouldNotThrow()
    {
        console.profileEnd("nonExistent");
    }

    [Fact]
    public void timeStamp_ShouldNotThrow()
    {
        console.timeStamp("testStamp");
        console.timeStamp();
    }

    [Fact]
    public void formatting_StringSubstitution()
    {
        console.log("Hello %s", "World");
        console.log("Multiple %s %s", "substitutions", "here");
    }

    [Fact]
    public void formatting_NumberSubstitution()
    {
        console.log("Number: %d", 42);
        console.log("Integer: %i", 123);
        console.log("Float: %f", 3.14);
    }

    [Fact]
    public void formatting_ObjectSubstitution()
    {
        console.log("Object: %o", new JSObject { ["name"] = "test", ["value"] = 42 });
        console.log("Object: %O", new JSObject { ["name"] = "test" });
    }

    [Fact]
    public void formatting_EscapedPercent()
    {
        console.log("Percentage: 50%%");
    }

    [Fact]
    public void formatting_ExtraParameters()
    {
        console.log("One %s", "param", "extra", "params");
        // Extra params should be appended
    }

    [Fact]
    public void formatting_NoParameters()
    {
        console.log("No substitution needed");
    }

    [Fact]
    public void multipleConsecutiveCalls_ShouldWork()
    {
        for (int i = 0; i < 10; i++)
        {
            console.log("Message %d", i);
        }
    }

    [Fact]
    public void mixedLoggingMethods_ShouldWork()
    {
        console.log("Log message");
        console.error("Error message");
        console.warn("Warning message");
        console.info("Info message");
        console.debug("Debug message");
    }

    [Fact]
    public void complexObjects_ShouldNotThrow()
    {
        var complexObj = new JSObject
        {
            ["name"] = "test",
            ["nested"] = new JSObject
            {
                ["value"] = 42,
                ["array"] = new TsArray(new object?[] { 1, 2, 3 })
            },
            ["nullValue"] = null
        };

        console.log("Complex object: %o", complexObj);
        console.dir(complexObj);
    }

    [Fact]
    public void counterScenario_ShouldTrackMultipleCounters()
    {
        console.countReset("api");
        console.countReset("db");
        console.countReset("cache");

        console.count("api");
        console.count("db");
        console.count("api");
        console.count("cache");
        console.count("api");

        // api: 3, db: 1, cache: 1
    }

    [Fact]
    public void timerScenario_ShouldHandleMultipleTimers()
    {
        console.time("timer1");
        console.time("timer2");

        Thread.Sleep(10);
        console.timeEnd("timer1");

        Thread.Sleep(10);
        console.timeEnd("timer2");
    }
}
