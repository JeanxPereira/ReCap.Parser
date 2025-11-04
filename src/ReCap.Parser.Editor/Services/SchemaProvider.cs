using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReCap.Parser.Core.Core;
using ReCap.Parser.Editor.Models;

namespace ReCap.Parser.Editor.Services
{
    public static class SchemaProvider
    {
        public static void Annotate(IList<Node> roots, Catalog catalog, string? rootElementName)
        {
            if (roots == null || roots.Count == 0) return;
            var root = roots[0];
            var rootStruct = ResolveStruct(catalog, rootElementName ?? root.Key);
            if (rootStruct == null) return;
            root.Kind = "struct";
            AnnotateStructChildren(root, rootStruct, catalog);
        }

        static void AnnotateStructChildren(Node structNode, StructDefinition def, Catalog catalog)
        {
            foreach (var child in structNode.Children)
            {
                var m = FindMember(def, child.Key);
                if (m == null) continue;

                if (string.Equals(m.TypeName, "array", StringComparison.OrdinalIgnoreCase))
                {
                    child.Kind = "array";
                    var elemStruct = catalog.GetStruct(m.ElementType);
                    var elemType = catalog.GetType(m.ElementType);
                    if (elemStruct != null)
                    {
                        foreach (var entry in child.Children)
                        {
                            entry.Kind = "struct";
                            AnnotateStructChildren(entry, elemStruct, catalog);
                        }
                    }
                    else if (elemType != null)
                    {
                        foreach (var entry in child.Children)
                        {
                            if (entry.Children.Count == 1)
                            {
                                var leaf = entry.Children[0];
                                leaf.Kind = MapDataTypeToKind(elemType);
                            }
                        }
                    }
                    continue;
                }

                var typeDef = catalog.GetType(m.TypeName);
                if (typeDef == null) continue;

                if (typeDef.Type == DataType.STRUCT)
                {
                    child.Kind = "struct";
                    var target = catalog.GetStruct(typeDef.TargetType);
                    if (target != null) AnnotateStructChildren(child, target, catalog);
                    continue;
                }

                if (typeDef.Type == DataType.NULLABLE)
                {
                    child.Kind = "nullable";
                    var target = catalog.GetStruct(typeDef.TargetType);
                    if (target != null) AnnotateStructChildren(child, target, catalog);
                    continue;
                }

                child.Kind = MapDataTypeToKind(typeDef);
            }
        }

        static StructMember? FindMember(StructDefinition def, string key)
        {
            foreach (var m in def.Members)
            {
                if (string.Equals(m.Name, key, StringComparison.OrdinalIgnoreCase)) return m;
            }
            return null;
        }

        static StructDefinition? ResolveStruct(Catalog catalog, string name)
        {
            var direct = catalog.GetStruct(name);
            if (direct != null) return direct;
            var cap = Capitalize(name);
            var s = catalog.GetStruct(cap);
            if (s != null) return s;
            var upper = name.ToUpperInvariant();
            s = catalog.GetStruct(upper);
            if (s != null) return s;
            var lower = name.ToLowerInvariant();
            s = catalog.GetStruct(lower);
            if (s != null) return s;
            if (string.Equals(name, "root", StringComparison.OrdinalIgnoreCase) && catalog.GetStruct("Noun") != null) return catalog.GetStruct("Noun");
            return null;
        }

        static string Capitalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length == 1) return s.ToUpperInvariant();
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }

        static string MapDataTypeToKind(TypeDefinition td)
        {
            switch (td.Type)
            {
                case DataType.BOOL: return "bool";
                case DataType.FLOAT: return "float";
                case DataType.INT: return "int32";
                case DataType.INT16: return "int16";
                case DataType.INT64: return "int64";
                case DataType.UINT8: return "uint8";
                case DataType.UINT16: return "uint16";
                case DataType.UINT32: return "uint32";
                case DataType.UINT64: return "uint64";
                case DataType.ENUM: return "enum";
                case DataType.KEY: return "key";
                case DataType.ASSET: return "asset";
                case DataType.CKEYASSET: return "ckeyasset";
                case DataType.GUID: return "guid";
                case DataType.VECTOR2: return "vector2";
                case DataType.VECTOR3: return "vector3";
                case DataType.QUATERNION: return "quaternion";
                case DataType.CHAR: return "string";
                case DataType.CHAR_PTR: return "string";
                case DataType.LOCALIZEDASSETSTRING: return "string";
                default: return "unknown";
            }
        }
    }
}
