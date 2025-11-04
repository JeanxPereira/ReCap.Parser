using System.Collections.Generic;
using System.Globalization;
using ReCap.Parser.Editor.Models;

namespace ReCap.Parser.Editor.Services
{
    public static class KeyAssetSuggestionsService
    {
        public static IEnumerable<string> Extract(IEnumerable<Node> roots)
        {
            var set = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var r in roots) Walk(r, set);
            return set;
        }

        static void Walk(Node n, HashSet<string> set)
        {
            if (n is StringValueNode sv && (n.Kind == "key" || n.Kind == "asset" || n.Kind == "ckeyasset"))
            {
                var s = sv.Value ?? "";
                if (!string.IsNullOrWhiteSpace(s)) set.Add(s);
            }
            foreach (var c in n.Children) Walk(c, set);
        }
    }
}
