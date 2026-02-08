using System.Reflection;
using Tracer.Core;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization;

public class SerializationUtils
{
    public static IEnumerable<ITraceResultSerializer> LoadSerializers(string serializersPath)
    {
        var serializers = new List<ITraceResultSerializer>();

        foreach (var dll in Directory.GetFiles(serializersPath, "*.dll"))
        {
            Assembly assembly = Assembly.LoadFrom(dll);

            foreach (var type in assembly.GetTypes())
            {
                if (typeof(ITraceResultSerializer).IsAssignableFrom(type) && !type.IsInterface 
                    && Activator.CreateInstance(type) is ITraceResultSerializer serializer)
                {
                    serializers.Add(serializer);
                }
            }
        }

        return serializers;
    }

    public static void SaveToFile(TraceResult traceResult, ITraceResultSerializer serializer, string filename, string directory)
    {
        string filePath = $"{directory}{Path.DirectorySeparatorChar}{filename}.{serializer.Format}";
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            serializer.Serialize(traceResult, fs);
        }
    }
}
