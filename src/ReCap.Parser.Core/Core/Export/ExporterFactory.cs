namespace ReCap.Parser.Core.Core;

using System;

public static class ExporterFactory
{
    public static IFormatExporter? CreateExporter(string format)
    {
        if (string.IsNullOrWhiteSpace(format)) return null;
        if (string.Equals(format, "none", StringComparison.OrdinalIgnoreCase)) return null;
        if (string.Equals(format, "xml", StringComparison.OrdinalIgnoreCase)) return new XmlExporter();
        if (string.Equals(format, "yaml", StringComparison.OrdinalIgnoreCase)) return new YamlExporter();
        if (string.Equals(format, "yml", StringComparison.OrdinalIgnoreCase)) return new YamlExporter();
        throw new NotSupportedException();
    }
}
