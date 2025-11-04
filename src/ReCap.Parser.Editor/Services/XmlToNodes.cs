using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using ReCap.Parser.Editor.Models;

namespace ReCap.Parser.Editor.Services
{
    public static class XmlToNodes
    {
        public static List<Node> FromXDocument(XDocument doc)
        {
            var list = new List<Node>();
            if (doc.Root == null) return list;
            if (doc.Root.Name.LocalName == "root")
            {
                foreach (var e in doc.Root.Elements())
                {
                    var n = Build(e, e.Name.LocalName);
                    list.Add(n);
                }
            }
            else
            {
                var n = Build(doc.Root, doc.Root.Name.LocalName);
                list.Add(n);
            }
            return list;
        }

        static Node Build(XElement e, string key)
        {
            if (!e.HasElements)
            {
                var text = e.Value ?? "";
                if (bool.TryParse(text, out var b)) return new BoolValueNode { Key = key, Value = b };
                if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) return new NumberValueNode { Key = key, Value = d };
                return new StringValueNode { Key = key, Value = text };
            }

            if (e.Elements().All(x => x.Name.LocalName == "entry"))
            {
                var arr = new ArrayNode { Key = key };
                foreach (var ent in e.Elements("entry")) AddChildren(arr, ent);
                return arr;
            }

            var s = new StructNode { Key = key };
            AddChildren(s, e);
            return s;
        }

        static void AddChildren(Node parent, XElement e)
        {
            foreach (var child in e.Elements())
            {
                var n = Build(child, child.Name.LocalName);
                n.Parent = parent;
                parent.Children.Add(n);
            }
        }
    }
}
