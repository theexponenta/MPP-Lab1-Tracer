using Tracer.Core;
using Tracer.Serialization.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class YamlTraceResultSerializer : ITraceResultSerializer
{
    public string Format => "yaml";

    public void Serialize(TraceResult traceResult, Stream to)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var dto = new
        {
            threads = traceResult.Threads.Select(t => new
            {
                id = t.ThreadId.ToString(),
                time = $"{t.Time}ms",
                methods = t.Methods.Select(m => ConvertMethodTraceResult(m)).ToList()
            }).ToList()
        };

        using var writer = new StreamWriter(to);
        serializer.Serialize(writer, dto);
    }

    private object ConvertMethodTraceResult(MethodTraceResult method)
    {
        return new
        {
            name = method.MethodName,
            @class = method.ClassName,
            time = $"{method.Time}ms",
            methods = method.Methods.Select(m => ConvertMethodTraceResult(m)).ToList()
        };
    }
}
