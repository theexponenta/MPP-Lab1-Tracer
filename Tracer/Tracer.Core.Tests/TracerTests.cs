using Xunit;
using Tracer.Core; 

namespace Tracer.Core.Tests;

public class TracerTests
{
    private void TraceSleep(ITracer tracer, int milliseconds)
    {
        tracer.StartTrace();
        Thread.Sleep(milliseconds);
        tracer.StopTrace();
    }

    private void NestedTraceSleep(ITracer tracer, int milliseconds, int depth)
    {
        if (depth == 0)
        {
            return;
        }

        tracer.StartTrace();

        Thread.Sleep(milliseconds);
        NestedTraceSleep(tracer, milliseconds, depth - 1);

        tracer.StopTrace();
    }

    [Fact]
    public void NoTracing_ShouldBeEmpty()
    {
        MethodTracer tracer = new MethodTracer();
        Assert.Empty(tracer.GetTraceResult().Threads);
    }

    [Fact]
    public void UnbalancedStack_ShouldThrowException()
    {
        Assert.Throws<InvalidOperationException>(() => { 
            MethodTracer tracer = new MethodTracer();
            tracer.StartTrace();
            tracer.GetTraceResult(); 
        });
    }

    [Fact]
    public void TraceUnnested()
    {
        MethodTracer tracer = new MethodTracer();
        int sleepTime = 50;

        TraceSleep(tracer, sleepTime);
        TraceResult result = tracer.GetTraceResult();

        Assert.Single(result.Threads);

        var thread = result.Threads[0];
        Assert.Single(thread.Methods);
        Assert.True(thread.Time >= sleepTime);
        Assert.Equal("Tracer.Core.Tests.TracerTests", thread.Methods[0].ClassName);
        Assert.Equal("TraceSleep", thread.Methods[0].MethodName);
    }

    [Fact]
    public void TestMultipleMethodsOnOneLevel()
    {
        MethodTracer tracer = new MethodTracer();
        int sleepTime = 20;

        TraceSleep(tracer, sleepTime);
        TraceSleep(tracer, sleepTime);
        TraceResult result = tracer.GetTraceResult();

        var thread = result.Threads[0];
        Assert.Equal(2, thread.Methods.Count);

        foreach (var method in thread.Methods)
        {
            Assert.True(method.Time >= sleepTime);
        }
    }

    [Fact]
    public void TestNested()
    {
        MethodTracer tracer = new MethodTracer();
        int sleepTime = 20;
        int depth = 3;

        NestedTraceSleep(tracer, sleepTime, depth);
        TraceResult result = tracer.GetTraceResult();

        var methods = result.Threads[0].Methods;
        for (int i = depth; i > 0; i--)
        {
            Assert.Single(methods);
            Assert.True(methods[0].Time >= sleepTime * i);
            methods = methods[0].Methods;
        }
    }

    [Fact]
    public void TestMultithreaded()
    {
        MethodTracer tracer = new MethodTracer();
        int sleepTime = 20;

        Thread thread = new Thread(() => {TraceSleep(tracer, sleepTime);});
        thread.Start();
        TraceSleep(tracer, 300);
        thread.Join();

        TraceResult traceResult = tracer.GetTraceResult();
        Assert.Equal(2, traceResult.Threads.Count);
    }
}
