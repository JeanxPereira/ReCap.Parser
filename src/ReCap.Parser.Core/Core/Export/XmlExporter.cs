namespace ReCap.Parser.Core.Core;

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

public sealed class XmlExporter : IFormatExporter
{
    private XDocument _doc = new();
    private readonly Stack<XContainer> _stack = new();

    public void BeginDocument()
    {
        _doc = new XDocument();
        _stack.Clear();
        _stack.Push(_doc);
    }

    public void EndDocument()
    {
        while (_stack.Count > 1) _stack.Pop();
    }

    public void BeginNode(string name)
    {
        var parent = _stack.Peek();
        var node = new XElement(name);
        parent.Add(node);
        _stack.Push(node);
    }

    public void EndNode()
    {
        if (_stack.Count > 1) _stack.Pop();
    }

    public void ExportBool(string name, bool value)
    {
        var parent = _stack.Peek();
        parent.Add(new XElement(name, value ? "true" : "false"));
    }

    public void ExportInt(string name, int value)
    {
        var parent = _stack.Peek();
        parent.Add(new XElement(name, value.ToString(CultureInfo.InvariantCulture)));
    }

    public void ExportUInt8(string name, byte value)
    {
        var parent = _stack.Peek();
        parent.Add(new XElement(name, value.ToString(CultureInfo.InvariantCulture)));
    }

    public void ExportUInt16(string name, ushort value)
    {
        var parent = _stack.Peek();
        parent.Add(new XElement(name, value.ToString(CultureInfo.InvariantCulture)));
    }

    public void ExportUInt32(string name, uint value)
    {
        var parent = _stack.Peek();
        parent.Add(new XElement(name, value.ToString(CultureInfo.InvariantCulture)));
    }

    public void ExportUInt64(string name, ulong value)
    {
        var parent = _stack.Peek();
        parent.Add(new XElement(name, "0x" + value.ToString("X", CultureInfo.InvariantCulture)));
    }

    public void ExportInt64(string name, long value)
    {
        var parent = _stack.Peek();
        var u = unchecked((ulong)value);
        parent.Add(new XElement(name, "0x" + u.ToString("X", CultureInfo.InvariantCulture)));
    }

    public void ExportFloat(string name, float value)
    {
        var parent = _stack.Peek();
        parent.Add(new XElement(name, value.ToString("0.00000", CultureInfo.InvariantCulture)));
    }

    public void ExportString(string name, string value)
    {
        var parent = _stack.Peek();
        parent.Add(new XElement(name, value));
    }

    public void ExportGuid(string name, string value)
    {
        var parent = _stack.Peek();
        parent.Add(new XElement(name, value));
    }

    public void ExportVector2(string name, float x, float y)
    {
        var parent = _stack.Peek();
        var n = new XElement(name);
        n.Add(new XElement("x", x.ToString("0.00000", CultureInfo.InvariantCulture)));
        n.Add(new XElement("y", y.ToString("0.00000", CultureInfo.InvariantCulture)));
        parent.Add(n);
    }

    public void ExportVector3(string name, float x, float y, float z)
    {
        var parent = _stack.Peek();
        var n = new XElement(name);
        n.Add(new XElement("x", x.ToString("0.00000", CultureInfo.InvariantCulture)));
        n.Add(new XElement("y", y.ToString("0.00000", CultureInfo.InvariantCulture)));
        n.Add(new XElement("z", z.ToString("0.00000", CultureInfo.InvariantCulture)));
        parent.Add(n);
    }

    public void ExportQuaternion(string name, float w, float x, float y, float z)
    {
        var parent = _stack.Peek();
        var n = new XElement(name);
        n.Add(new XElement("w", w.ToString("0.00000", CultureInfo.InvariantCulture)));
        n.Add(new XElement("x", x.ToString("0.00000", CultureInfo.InvariantCulture)));
        n.Add(new XElement("y", y.ToString("0.00000", CultureInfo.InvariantCulture)));
        n.Add(new XElement("z", z.ToString("0.00000", CultureInfo.InvariantCulture)));
        parent.Add(n);
    }

    public void BeginArray(string name)
    {
        var parent = _stack.Peek();
        var n = new XElement(name);
        parent.Add(n);
        _stack.Push(n);
    }

    public void BeginArrayEntry()
    {
        var parent = _stack.Peek();
        var n = new XElement("entry");
        parent.Add(n);
        _stack.Push(n);
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
        _doc.Save(filepath);
        return true;
    }
}
