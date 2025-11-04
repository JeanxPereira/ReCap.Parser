namespace ReCap.Parser.Core.Core;

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using YamlDotNet.RepresentationModel;

public sealed class YamlExporter : IFormatExporter
{
    private YamlMappingNode _root = new();
    private readonly Stack<YamlNode> _stack = new();

    public void BeginDocument()
    {
        _root = new YamlMappingNode();
        _stack.Clear();
        _stack.Push(_root);
    }

    public void EndDocument()
    {
        while (_stack.Count > 1) _stack.Pop();
    }

    public void BeginNode(string name)
    {
        var current = (YamlMappingNode)_stack.Peek();
        var child = new YamlMappingNode();
        current.Add(new YamlScalarNode(name), child);
        _stack.Push(child);
    }

    public void EndNode()
    {
        if (_stack.Count > 1) _stack.Pop();
    }

    public void ExportBool(string name, bool value)
    {
        var current = (YamlMappingNode)_stack.Peek();
        current.Add(new YamlScalarNode(name), new YamlScalarNode(value ? "true" : "false"));
    }

    public void ExportInt(string name, int value)
    {
        var current = (YamlMappingNode)_stack.Peek();
        current.Add(new YamlScalarNode(name), new YamlScalarNode(value.ToString(CultureInfo.InvariantCulture)));
    }

    public void ExportUInt8(string name, byte value)
    {
        var current = (YamlMappingNode)_stack.Peek();
        current.Add(new YamlScalarNode(name), new YamlScalarNode(value.ToString(CultureInfo.InvariantCulture)));
    }

    public void ExportUInt16(string name, ushort value)
    {
        var current = (YamlMappingNode)_stack.Peek();
        current.Add(new YamlScalarNode(name), new YamlScalarNode(value.ToString(CultureInfo.InvariantCulture)));
    }

    public void ExportUInt32(string name, uint value)
    {
        var current = (YamlMappingNode)_stack.Peek();
        current.Add(new YamlScalarNode(name), new YamlScalarNode(value.ToString(CultureInfo.InvariantCulture)));
    }

    public void ExportUInt64(string name, ulong value)
    {
        var current = (YamlMappingNode)_stack.Peek();
        current.Add(new YamlScalarNode(name), new YamlScalarNode("0x" + value.ToString("X", CultureInfo.InvariantCulture)));
    }

    public void ExportInt64(string name, long value)
    {
        var current = (YamlMappingNode)_stack.Peek();
        var u = unchecked((ulong)value);
        current.Add(new YamlScalarNode(name), new YamlScalarNode("0x" + u.ToString("X", CultureInfo.InvariantCulture)));
    }

    public void ExportFloat(string name, float value)
    {
        var current = (YamlMappingNode)_stack.Peek();
        current.Add(new YamlScalarNode(name), new YamlScalarNode(value.ToString("0.00000", CultureInfo.InvariantCulture)));
    }

    public void ExportString(string name, string value)
    {
        var current = (YamlMappingNode)_stack.Peek();
        current.Add(new YamlScalarNode(name), new YamlScalarNode(value));
    }

    public void ExportGuid(string name, string value)
    {
        var current = (YamlMappingNode)_stack.Peek();
        current.Add(new YamlScalarNode(name), new YamlScalarNode(value));
    }

    public void ExportVector2(string name, float x, float y)
    {
        var current = (YamlMappingNode)_stack.Peek();
        var m = new YamlMappingNode();
        m.Add("x", x.ToString("0.00000", CultureInfo.InvariantCulture));
        m.Add("y", y.ToString("0.00000", CultureInfo.InvariantCulture));
        current.Add(new YamlScalarNode(name), m);
    }

    public void ExportVector3(string name, float x, float y, float z)
    {
        var current = (YamlMappingNode)_stack.Peek();
        var m = new YamlMappingNode();
        m.Add("x", x.ToString("0.00000", CultureInfo.InvariantCulture));
        m.Add("y", y.ToString("0.00000", CultureInfo.InvariantCulture));
        m.Add("z", z.ToString("0.00000", CultureInfo.InvariantCulture));
        current.Add(new YamlScalarNode(name), m);
    }

    public void ExportQuaternion(string name, float w, float x, float y, float z)
    {
        var current = (YamlMappingNode)_stack.Peek();
        var m = new YamlMappingNode();
        m.Add("w", w.ToString("0.00000", CultureInfo.InvariantCulture));
        m.Add("x", x.ToString("0.00000", CultureInfo.InvariantCulture));
        m.Add("y", y.ToString("0.00000", CultureInfo.InvariantCulture));
        m.Add("z", z.ToString("0.00000", CultureInfo.InvariantCulture));
        current.Add(new YamlScalarNode(name), m);
    }

    public void BeginArray(string name)
    {
        var current = (YamlMappingNode)_stack.Peek();
        var seq = new YamlSequenceNode();
        current.Add(new YamlScalarNode(name), seq);
        _stack.Push(seq);
    }

    public void BeginArrayEntry()
    {
        var seq = (YamlSequenceNode)_stack.Peek();
        var entry = new YamlMappingNode();
        seq.Add(entry);
        _stack.Push(entry);
    }

    public void EndArrayEntry()
    {
        if (_stack.Count > 1) _stack.Pop();
    }

    public void EndArray()
    {
        if (_stack.Count > 1) _stack.Pop();
    }

    public bool SaveToFile(string filepath)
    {
        using var writer = new StreamWriter(filepath);
        var stream = new YamlStream(new YamlDocument(_root));
        stream.Save(writer, false);
        return true;
    }
}
