namespace Tracer.Core;

public class MethodTraceResult
{
    public long Time {get; internal set;}
    public string ClassName {get; private set;}
    public string MethodName {get; private set;}
    private List<MethodTraceResult> _methods;
    public IReadOnlyList<MethodTraceResult> Methods
    {
        get
        {
            return _methods.AsReadOnly();
        }
    }

    internal MethodTraceResult(string className, string methodName)
    {
        ClassName = className;
        MethodName = methodName;
        _methods = new List<MethodTraceResult>();
    }

    internal void AddMethodTraceResult(MethodTraceResult methodTraceResult)
    {
        _methods.Add(methodTraceResult);
    }
} 
