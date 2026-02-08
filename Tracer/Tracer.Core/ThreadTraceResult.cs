using System.Collections;
namespace Tracer.Core;

public class ThreadTraceResult
{
    public int ThreadId {get; private set;}
    private List<MethodTraceResult> _methods;
    public IReadOnlyList<MethodTraceResult> Methods
    {
        get
        {
            return _methods.AsReadOnly();
        }
    }

    public long Time
    {
        get
        {
            long result = 0;
            foreach (var method in _methods)
            {
                result += method.Time;
            }

            return result;
        }
    }

    internal ThreadTraceResult(int threadId)
    {
        ThreadId = threadId;
        _methods = new List<MethodTraceResult>();
    }

    internal void AddMethodTraceResult(MethodTraceResult methodTraceResult)
    {
        _methods.Add(methodTraceResult);
    }
}
