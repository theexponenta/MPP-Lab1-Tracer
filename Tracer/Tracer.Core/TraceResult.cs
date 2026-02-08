using System.Collections.ObjectModel;

namespace Tracer.Core;

public class TraceResult
{
    public IReadOnlyList<ThreadTraceResult> Threads { get; private set; }

    internal TraceResult(IReadOnlyList<ThreadTraceResult> threadTraceResults)
    {
        Threads = threadTraceResults;
    }
}
