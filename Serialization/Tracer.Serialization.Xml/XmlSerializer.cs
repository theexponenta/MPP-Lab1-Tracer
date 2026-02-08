using System.Xml.Linq;
using Tracer.Core;
using Tracer.Serialization.Abstractions;

public class XmlTraceResultSerializer : ITraceResultSerializer
{
    public string Format => "xml";

    public void Serialize(TraceResult traceResult, Stream to)
    {
        var root = new XElement("threads",
            traceResult.Threads.Select(t =>
                new XElement("thread",
                    new XAttribute("id", t.ThreadId),
                    new XAttribute("time", $"{t.Time}ms"),
                    new XElement("methods",
                        t.Methods.Select(m => ConvertMethodTraceResult(m))
                    )
                )
            )
        );

        var doc = new XDocument(root);
        doc.Save(to);
    }

    private XElement ConvertMethodTraceResult(MethodTraceResult method)
    {
        return new XElement("method",
            new XAttribute("name", method.MethodName),
            new XAttribute("class", method.ClassName),
            new XAttribute("time", $"{method.Time}ms"),
            new XElement("methods",
                method.Methods.Select(m => ConvertMethodTraceResult(m))
            )
        );
    }
}

