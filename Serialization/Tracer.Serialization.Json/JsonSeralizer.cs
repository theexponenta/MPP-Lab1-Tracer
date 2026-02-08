using System.Text.Json;
using Tracer.Core;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization.Json;

public class JsonTraceResultSerializer : ITraceResultSerializer
{
    public string Format => "json";

    public void Serialize(TraceResult traceResult, Stream to)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var dto = new
        {
            threads = traceResult.Threads.Select(t => new
            {
                id = t.ThreadId.ToString(),
                time = $"{t.Time}ms",
                methods = t.Methods.Select(m => ConvertMethodTraceResult(m))
            })
        };

        JsonSerializer.Serialize(to, dto, options);
    }

    private object ConvertMethodTraceResult(MethodTraceResult method)
    {
        return new
        {
            name = method.MethodName,
            @class = method.ClassName,
            time = $"{method.Time}ms",
            methods = method.Methods.Select(m => ConvertMethodTraceResult(m))
        };
    }
}

