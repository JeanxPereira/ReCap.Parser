using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ReCap.Parser.Editor.Models;

namespace ReCap.Parser.Editor.Services
{
    public static class NodesToXml
    {
        public static XDocument Build(IEnumerable<Node> roots)
        {
            var list = roots?.ToList() ?? new List<Node>();
            if (list.Count == 1) return new XDocument(ToElement(list[0]));
            var root = new XElement("root");
            foreach (var n in list) root.Add(ToElement(n));
            return new XDocument(root);
        }

        static XElement ToElement(Node n)
        {
            if (n is StringValueNode sv) return new XElement(sv.Key, sv.Value ?? "");
            if (n is BoolValueNode bv) return new XElement(bv.Key, bv.Value ? "true" : "false");
            if (n is NumberValueNode nv) return new XElement(nv.Key, nv.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (n is ArrayNode an)
            {
                var e = new XElement(an.Key);
                foreach (var c in an.Children) e.Add(WrapEntry(c));
                return e;
            }
            var s = new XElement(n.Key);
            foreach (var c in n.Children) s.Add(ToElement(c));
            return s;
        }

        static XElement WrapEntry(Node n)
        {
            var entry = new XElement("entry");
            if (n is StringValueNode sv) entry.Add(new XElement(sv.Key, sv.Value ?? ""));
            else if (n is BoolValueNode bv) entry.Add(new XElement(bv.Key, bv.Value ? "true" : "false"));
            else if (n is NumberValueNode nv) entry.Add(new XElement(nv.Key, nv.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            else if (n is ArrayNode an)
            {
                var e = new XElement(an.Key);
                foreach (var c in an.Children) e.Add(WrapEntry(c));
                entry.Add(e);
            }
            else
            {
                var s = new XElement(n.Key);
                foreach (var c in n.Children) s.Add(ToElement(c));
                entry.Add(s);
            }
            return entry;
        }
    }
}
