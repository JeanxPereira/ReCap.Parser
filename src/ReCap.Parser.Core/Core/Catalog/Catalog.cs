namespace ReCap.Parser.Core.Core;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public enum DataType
{
    BOOL,
    INT,
    INT16,
    INT64,
    UINT8,
    UINT16,
    UINT32,
    UINT64,
    FLOAT,
    GUID,
    VECTOR2,
    VECTOR3,
    QUATERNION,
    LOCALIZEDASSETSTRING,
    CHAR,
    CHAR_PTR,
    KEY,
    ASSET,
    CKEYASSET,
    NULLABLE,
    ARRAY,
    ENUM,
    STRUCT
}

public sealed class TypeDefinition
{
    public string Name { get; }
    public DataType Type { get; }
    public int Size { get; }
    public string TargetType { get; }

    public TypeDefinition(string name, DataType type, int size)
    {
        Name = name;
        Type = type;
        Size = size;
        TargetType = string.Empty;
    }

    public TypeDefinition(string name, DataType type, int size, string targetType)
    {
        Name = name;
        Type = type;
        Size = size;
        TargetType = targetType;
    }
}

public sealed class StructMember
{
    public string Name { get; }
    public string TypeName { get; }
    public int Offset { get; }
    public bool UseSecondaryOffset { get; }
    public string ElementType { get; }
    public bool HasCustomName { get; }
    public int CountOffset { get; }

    public StructMember(string name, string typeName, int offset, bool useSecondaryOffset = false, bool hasCustomName = false, int countOffset = 0)
    {
        Name = name;
        TypeName = typeName;
        Offset = offset;
        UseSecondaryOffset = useSecondaryOffset;
        ElementType = string.Empty;
        HasCustomName = hasCustomName;
        CountOffset = countOffset;
    }

    public StructMember(string name, string typeName, string elementType, int offset, bool useSecondaryOffset = false, bool hasCustomName = false, int countOffset = 0)
    {
        Name = name;
        TypeName = typeName;
        Offset = offset;
        UseSecondaryOffset = useSecondaryOffset;
        ElementType = elementType;
        HasCustomName = hasCustomName;
        CountOffset = countOffset;
    }
}

public sealed class StructDefinition
{
    public string Name { get; }
    public int FixedSize { get; }
    private readonly List<StructMember> _members = new();

    public StructDefinition(string name, int fixedSize = 0)
    {
        Name = name;
        FixedSize = fixedSize;
    }

    public IReadOnlyList<StructMember> Members => _members;

    public StructDefinition Add(string name, string typeName, int offset, bool useSecondaryOffset = false)
    {
        _members.Add(new StructMember(name, typeName, offset, useSecondaryOffset));
        return this;
    }

    public StructDefinition AddArray(string name, string elementType, int offset, bool useSecondaryOffset = false, int countOffset = 0)
    {
        _members.Add(new StructMember(name, "array", elementType, offset, useSecondaryOffset, false, countOffset));
        return this;
    }

    public StructDefinition AddStructArray(string name, StructDefinition elementStruct, int offset, int countOffset = 0, bool useSecondaryOffset = false)
    {
        _members.Add(new StructMember(name, "array", elementStruct.Name, offset, useSecondaryOffset, false, countOffset));
        return this;
    }

    public StructDefinition Add(StructDefinition structDef, int offset)
    {
        _members.Add(new StructMember(structDef.Name, "struct:" + structDef.Name, offset, false));
        return this;
    }

    public StructDefinition Add(string name, StructDefinition structDef, int offset)
    {
        _members.Add(new StructMember(name, "struct:" + structDef.Name, offset, false, true));
        return this;
    }

    public StructDefinition Add(string name, string typeName, StructDefinition targetStruct, int offset, bool useSecondaryOffset = false)
    {
        if (typeName == "nullable")
        {
            var specificType = "nullable:" + targetStruct.Name;
            var custom = name != targetStruct.Name;
            _members.Add(new StructMember(name, specificType, offset, useSecondaryOffset, custom));
        }
        else
        {
            _members.Add(new StructMember(name, typeName, offset, useSecondaryOffset));
        }
        return this;
    }
}

public sealed class VersionedFileTypeInfo
{
    public string Version { get; }
    public List<string> StructTypes { get; }
    public int SecondaryOffsetStart { get; }

    public VersionedFileTypeInfo(string version, IEnumerable<string> types, int secondaryOffsetStart = 0)
    {
        Version = version;
        StructTypes = types.ToList();
        SecondaryOffsetStart = secondaryOffsetStart;
    }
}

public sealed class FileTypeInfo
{
    public List<string> StructTypes { get; }
    public int SecondaryOffsetStart { get; }
    public List<VersionedFileTypeInfo> VersionedInfo { get; }

    public FileTypeInfo(IEnumerable<string> types, int secondaryOffsetStart = 0)
    {
        StructTypes = types.ToList();
        SecondaryOffsetStart = secondaryOffsetStart;
        VersionedInfo = new List<VersionedFileTypeInfo>();
    }

    public FileTypeInfo(IEnumerable<VersionedFileTypeInfo> versions)
    {
        StructTypes = new List<string>();
        SecondaryOffsetStart = 0;
        VersionedInfo = versions.ToList();
    }
}

public sealed partial class Catalog
{
    private readonly Dictionary<string, TypeDefinition> _types = new(StringComparer.Ordinal);
    private readonly Dictionary<string, StructDefinition> _structs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FileTypeInfo> _fileTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FileTypeInfo> _exactFileNames = new(StringComparer.OrdinalIgnoreCase);
    private string _currentGameVersion = "5.3.0.103";

    public Catalog()
    {
        Initialize();
    }

    public void SetGameVersion(string version)
    {
        _currentGameVersion = version;
    }

    public string GetGameVersion() => _currentGameVersion;

    public TypeDefinition AddType(string name, DataType type, int size)
    {
        var def = new TypeDefinition(name, type, size);
        _types[name] = def;
        return def;
    }

    public TypeDefinition AddArrayType(string elementType, int size)
    {
        var name = "array:" + elementType;
        var def = new TypeDefinition(name, DataType.ARRAY, size, elementType);
        _types[name] = def;
        return def;
    }

    public TypeDefinition AddType(string name, DataType type, int size, string targetType)
    {
        var def = new TypeDefinition(name, type, size, targetType);
        _types[name] = def;
        return def;
    }

    public StructDefinition AddStruct(string name, int fixedSize = 0)
    {
        var sd = new StructDefinition(name, fixedSize);
        _structs[name] = sd;
        AddType("struct:" + name, DataType.STRUCT, fixedSize, name);
        RegisterNullableType(name);
        return sd;
    }

    public void RegisterFileType(string extension, IEnumerable<string> structTypes, int secondaryOffsetStart = 0)
    {
        var key = extension;
        _fileTypes[key] = new FileTypeInfo(structTypes, secondaryOffsetStart);
    }

    public void RegisterFileType(string extension, IEnumerable<VersionedFileTypeInfo> versions)
    {
        var key = extension;
        _fileTypes[key] = new FileTypeInfo(versions);
    }

    public void RegisterFileName(string fileName, IEnumerable<string> structTypes, int secondaryOffsetStart = 0)
    {
        var key = fileName;
        _exactFileNames[key] = new FileTypeInfo(structTypes, secondaryOffsetStart);
    }

    public void RegisterFileName(string fileName, IEnumerable<VersionedFileTypeInfo> versions)
    {
        var key = fileName;
        _exactFileNames[key] = new FileTypeInfo(versions);
    }

    public void RegisterNullableType(string targetStructName)
    {
        var name = "nullable:" + targetStructName;
        if (!_types.ContainsKey(name))
        {
            AddType(name, DataType.NULLABLE, 4, targetStructName);
        }
    }

    public TypeDefinition? GetType(string name)
    {
        return _types.TryGetValue(name, out var t) ? t : null;
    }

    public StructDefinition? GetStruct(string name)
    {
        return _structs.TryGetValue(name, out var s) ? s : null;
    }

    public FileTypeInfo? GetFileType(string extension)
    {
        if (_fileTypes.TryGetValue(extension, out var f1)) return f1;
        if (extension.StartsWith(".")) return _fileTypes.TryGetValue(extension, out var f2) ? f2 : null;
        var dot = "." + extension;
        return _fileTypes.TryGetValue(dot, out var f3) ? f3 : null;
    }

    public FileTypeInfo? GetFileTypeByName(string filename)
    {
        var name = Path.GetFileName(filename);
        return _exactFileNames.TryGetValue(name, out var v) ? v : null;
    }

    public VersionedFileTypeInfo? GetVersionedFileTypeInfo(FileTypeInfo? fileTypeInfo)
    {
        if (fileTypeInfo == null || fileTypeInfo.VersionedInfo.Count == 0) return null;
        foreach (var v in fileTypeInfo.VersionedInfo)
        {
            if (string.Equals(v.Version, _currentGameVersion, StringComparison.Ordinal)) return v;
        }
        return fileTypeInfo.VersionedInfo[0];
    }

    private void Initialize()
    {
        AddType("bool", DataType.BOOL, 1);
        AddType("int", DataType.INT, 4);
        AddType("int16_t", DataType.INT16, 2);
        AddType("int64_t", DataType.INT64, 8);
        AddType("uint8_t", DataType.UINT8, 1);
        AddType("uint16_t", DataType.UINT16, 2);
        AddType("uint32_t", DataType.UINT32, 4);
        AddType("uint64_t", DataType.UINT64, 8);
        AddType("float", DataType.FLOAT, 4);
        AddType("guid", DataType.GUID, 16);
        AddType("cSPVector2", DataType.VECTOR2, 8);
        AddType("cSPVector3", DataType.VECTOR3, 12);
        AddType("cSPVector4", DataType.QUATERNION, 16);
        AddType("cLocalizedAssetString", DataType.LOCALIZEDASSETSTRING, 4);
        AddType("char", DataType.CHAR, 1);
        AddType("char*", DataType.CHAR_PTR, 4);
        AddType("key", DataType.KEY, 4);
        AddType("asset", DataType.ASSET, 4);
        AddType("cKeyAsset", DataType.CKEYASSET, 16);
        AddType("nullable", DataType.NULLABLE, 4);
        AddType("array", DataType.ARRAY, 4);
        AddType("enum", DataType.ENUM, 4);
        Register_Noun();
    }

    partial void Register_Noun();
}
