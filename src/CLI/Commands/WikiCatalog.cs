using System.Text;
using ReCap.Parser;

namespace ReCap.Parser.CLI.Commands;

public static class WikiCatalog
{
    private static readonly Dictionary<string, HashSet<string>> _usagesMap = new();
    private const string FolderStructures = "Structures";
    private const string FolderEnums = "Enums";
    private const string FolderCatalog = "Catalog";

    public static void Run(string outputDir)
    {
        Console.WriteLine($"[WikiGen] Generating Wiki in: {outputDir}");

        var catalogDir = Path.Combine(outputDir, FolderCatalog);
        var structsDir = Path.Combine(catalogDir, FolderStructures);
        var enumsDir = Path.Combine(catalogDir, FolderEnums);

        if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
        Directory.CreateDirectory(structsDir);
        Directory.CreateDirectory(enumsDir);

        var assembly = typeof(AssetCatalog).Assembly;
        var catalogTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(AssetCatalog)) && !t.IsAbstract)
            .OrderBy(t => t.Name);

        var loadedCatalogs = new List<AssetCatalog>();
        foreach (var type in catalogTypes)
        {
            try { loadedCatalogs.Add((AssetCatalog)Activator.CreateInstance(type)!); }
            catch { Console.WriteLine($"[WikiGen] Warning: Failed to instantiate {type.Name}"); }
        }

        BuildDependencyGraph(loadedCatalogs);

        var allStructs = new List<string>();
        var allEnums = new List<string>();

        foreach (var catalog in loadedCatalogs)
        {
            foreach (var structName in catalog.StructNames)
            {
                var def = catalog.GetStruct(structName);
                if (def == null) continue;
                var markdown = GenerateMarkdownForStruct(def);
                File.WriteAllText(Path.Combine(structsDir, $"{def.Name}.md"), markdown);
                allStructs.Add(def.Name);
            }

            foreach (var enumName in catalog.EnumNames)
            {
                var def = catalog.GetEnum(enumName);
                if (def == null) continue;
                var markdown = GenerateMarkdownForEnum(def);
                File.WriteAllText(Path.Combine(enumsDir, $"{def.Name}.md"), markdown);
                allEnums.Add(def.Name);
            }
        }

        GenerateSidebar(outputDir, allStructs, allEnums);
        Console.WriteLine($"[WikiGen] Done! {allStructs.Count} Structures, {allEnums.Count} Enums.");
    }

    private static void GenerateSidebar(string outputDir, List<string> structs, List<string> enums)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("## Base");
        sb.AppendLine("* **[Home](https://github.com/JeanxPereira/ReCap.Parser/wiki/Home )**");
        sb.AppendLine();

        sb.AppendLine("## Catalog Assets");
        
        sb.AppendLine("  * **Structures**");
        foreach (var s in structs.OrderBy(x => x))
        {
            sb.AppendLine($"    * [[{s}]]");
        }
        
        sb.AppendLine("  * **Enums**");
        foreach (var e in enums.OrderBy(x => x))
        {
            sb.AppendLine($"    * [[{e}]]");
        }

        File.WriteAllText(Path.Combine(outputDir, "_Sidebar.md"), sb.ToString());
    }

    private static void BuildDependencyGraph(List<AssetCatalog> catalogs)
    {
        _usagesMap.Clear();
        foreach (var catalog in catalogs)
        {
            foreach (var structName in catalog.StructNames)
            {
                var definition = catalog.GetStruct(structName);
                if (definition == null) continue;

                foreach (var field in definition.Fields)
                {
                    string? dep = null;
                    if (field.Type == DataType.Struct) dep = field.ElementType;
                    else if (field.Type == DataType.Enum) dep = field.EnumType;
                    else if (field.Type == DataType.Array && field.ElementType != null)
                    {
                         if (!Enum.TryParse<DataType>(field.ElementType, true, out _))
                            dep = field.ElementType;
                    }

                    if (dep != null)
                    {
                        if (!_usagesMap.ContainsKey(dep)) _usagesMap[dep] = new HashSet<string>();
                        _usagesMap[dep].Add(structName);
                    }
                }
            }
        }
    }

    private static string GenerateMarkdownForStruct(StructDefinition def)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {def.Name}");
        sb.AppendLine($"**Size:** `0x{def.Size:x}`");
        sb.AppendLine($"**Count:** `0x{def.Fields.Count:x}`");
        sb.AppendLine();

        sb.AppendLine("## Structure");
        sb.AppendLine("| Offset | DataType | Name |");
        sb.AppendLine("| :--- | :--- | :--- |");

        foreach (var field in def.Fields)
        {
            sb.AppendLine($"| `0x{field.Offset:X2}` | {FormatDataType(field)} | **{field.Name}** |");
        }
        sb.AppendLine();

        AppendReferencesSection(sb, def.Name);
        return sb.ToString();
    }

    private static string GenerateMarkdownForEnum(EnumDefinition def)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {def.Name}");
        sb.AppendLine();
        sb.AppendLine("## Values");
        sb.AppendLine("| Value | Name |");
        sb.AppendLine("| :--- | :--- |");

        foreach (var kvp in def.Values.OrderBy(x => x.Key))
        {
            sb.AppendLine($"| `0x{kvp.Key:X8}` | **{kvp.Value}** |");
        }
        sb.AppendLine();

        AppendReferencesSection(sb, def.Name);
        return sb.ToString();
    }

    private static void AppendReferencesSection(StringBuilder sb, string typeName)
    {
        if (_usagesMap.TryGetValue(typeName, out var users) && users.Count > 0)
        {
            sb.AppendLine("---");
            sb.AppendLine("## Reference");
            sb.AppendLine("> Used by:");
            sb.AppendLine();
            foreach (var user in users.OrderBy(u => u))
            {
                sb.AppendLine($"- [`{user}`]({user})");
            }
            sb.AppendLine();
        }
    }

    private static string FormatDataType(FieldDefinition field)
    {
        if (field.Type == DataType.Struct)
        {
            var name = field.ElementType ?? "Unknown";
            return $"[`{name}`]({name})";
        }
        if (field.Type == DataType.Enum)
        {
            var name = field.EnumType ?? "Unknown";
            return field.EnumType != null 
                ? $"`Enum<`[`{name}`]({name})`>`"
                : "`Enum`";
        }
        if (field.Type == DataType.Array)
        {
            var inner = field.ElementType ?? "Unknown";
            bool isComplexType = !Enum.TryParse<DataType>(inner, true, out _);
            
            if (isComplexType)
                return $"`Array<`[`{inner}`]({inner})`>`";
            else
                return $"`Array<{inner.ToLower()}>`";
        }
        return $"`{field.Type.ToString().ToLower()}`";
    }
}
