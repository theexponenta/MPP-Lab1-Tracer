using System.Diagnostics;
using Tracer.Core;
using Tracer.Serialization;

namespace Tracer.Example;

public class Foo
{
    private Bar _bar;
    private ITracer _tracer;

    internal Foo(ITracer tracer)
    {
        _tracer = tracer;
        _bar = new Bar(_tracer);
    }
    
    public void MyMethod()
    {
        _tracer.StartTrace();
        Thread.Sleep(100);
        _bar.InnerMethod();
        _tracer.StopTrace();
    }
}

public class Bar
{
    private ITracer _tracer;

    internal Bar(ITracer tracer)
    {
        _tracer = tracer;
    }
    
    public void InnerMethod()
    {
        _tracer.StartTrace();
        Thread.Sleep(200);
        _tracer.StopTrace();
    }
}

class Program
{
    private const string SOURCE_DIR_NAME = "Tracer.Example";

    private static string GetSourceDirPath()
    {
        string baseDirectory = AppContext.BaseDirectory;
        int sourceDirIndex = baseDirectory.LastIndexOf(SOURCE_DIR_NAME);
        return baseDirectory.Substring(0, sourceDirIndex);
    }

    private static string GetSerializersPath()
    {
        return $"{GetSourceDirPath()}..{Path.DirectorySeparatorChar}Serializers";
    }

    private static string GetSerializationResultsDirPath()
    {
        return $"{GetSourceDirPath()}..{Path.DirectorySeparatorChar}SerializationResults";
    }

    static void Main(string[] args)
    {        
        MethodTracer tracer = new MethodTracer();
        Foo foo = new Foo(tracer);
        foo.MyMethod();

        Thread thread1 = new Thread(foo.MyMethod);
        Thread thread2 = new Thread(foo.MyMethod);
        tracer.StartTrace();
        thread1.Start();
        thread2.Start();
        thread1.Join();
        thread2.Join();
        tracer.StopTrace();

        TraceResult result = tracer.GetTraceResult();

        string serializersPath = GetSerializersPath();

        var serializers = SerializationUtils.LoadSerializers(serializersPath);
        foreach (var serializer in serializers)
        {
            SerializationUtils.SaveToFile(result, serializer, "result", GetSerializationResultsDirPath());
        }
    }
}
