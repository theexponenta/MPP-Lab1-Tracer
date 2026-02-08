using System.Diagnostics;
using System.Reflection;
using System.Collections.Concurrent;

namespace Tracer.Core;

public class MethodTracer : ITracer
{
    private class StackEntry
    {
        public Stopwatch Stopwatch {get; set;}
        public MethodTraceResult MethodTraceResult {get; set;}

        internal StackEntry(Stopwatch stopwatch, MethodTraceResult methodTraceResult)
        {
            Stopwatch = stopwatch;
            MethodTraceResult = methodTraceResult;
        }
    }

    private ConcurrentDictionary<int, ThreadTraceResult> _results;

    private ConcurrentDictionary<int, Stack<StackEntry>> _stacks;

    public MethodTracer()
    {
        _results = new ConcurrentDictionary<int, ThreadTraceResult>();
        _stacks = new ConcurrentDictionary<int, Stack<StackEntry>>();
    }

    private Stack<StackEntry> GetCurrentThreadStack()
    {
        return _stacks.GetOrAdd(Environment.CurrentManagedThreadId, id => { return new Stack<StackEntry>(); });
    }

    private ThreadTraceResult GetCurrentThreadResult()
    {
        return _results.GetOrAdd(Environment.CurrentManagedThreadId, id => { return new ThreadTraceResult(id); });
    }

    public void StartTrace()
    {
        StackTrace st = new StackTrace(false);
        StackFrame caller = st.GetFrame(1)!;

        MethodBase method = caller.GetMethod()!;
        MethodTraceResult methodTraceResult = new MethodTraceResult(method.Name, method.DeclaringType!.FullName!);

        Stack<StackEntry> stack = GetCurrentThreadStack();
        if (stack.Count == 0)
        {
            ThreadTraceResult threadResult = GetCurrentThreadResult();
            threadResult.AddMethodTraceResult(methodTraceResult);
        } 
        else
        {
            stack.Peek().MethodTraceResult.AddMethodTraceResult(methodTraceResult);
        }

        StackEntry stackEntry = new StackEntry(new Stopwatch(), methodTraceResult);
        stack.Push(stackEntry);
        stackEntry.Stopwatch.Start();
    }

    public void StopTrace()
    {
        Stack<StackEntry> stack = GetCurrentThreadStack();
        StackEntry stackEntry = stack.Pop();

        stackEntry.Stopwatch.Stop();
        stackEntry.MethodTraceResult.Time = stackEntry.Stopwatch.ElapsedMilliseconds;
    }

    public TraceResult GetTraceResult()
    {
        foreach (var (threadId, stack) in _stacks)
        {
            if (stack.Count > 0)
            {   
                throw new Exception($"Tracing of thread {threadId} is not completed");
            }
        }

        return new TraceResult(_results.Values.ToList().AsReadOnly());
    }
}
