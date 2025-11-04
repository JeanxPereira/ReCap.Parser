namespace ReCap.Parser.Core.Core;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public sealed class Parser
{
    private readonly Catalog _catalog;
    private readonly string _filename;
    private readonly bool _silentMode;
    private readonly bool _debugMode;
    private readonly bool _exportMode;
    private readonly IFormatExporter? _exporter;

    private FileStream? _fs;
    private OffsetManager? _offset;
    private OffsetManager O => _offset!;

    private long _totalArraySize;
    private bool _isInsideNullable;
    private bool _secOffsetStruct;
    private readonly Stack<long> _structOffsetStack = new();
    // private long _structBaseStartOffset;
    private long _startNullableOffset;
    // private long _structBaseOffset;
    private long _currentStructBaseOffset;
    private readonly Stack<long> _structBaseOffsetStack = new();
    private bool _processingArrayElement;
    private bool _isProcessingRootTag;
    private int _indentLevel;

    public Parser(Catalog catalog, string filename, bool silentMode = true, bool debugMode = false, string exportFormat = "xml")
    {
        _catalog = catalog;
        _filename = filename;
        _silentMode = silentMode;
        _debugMode = debugMode;
        _exportMode = !string.Equals(exportFormat, "none", StringComparison.OrdinalIgnoreCase);
        _exporter = ExporterFactory.CreateExporter(exportFormat);
    }

    private string Indent() => new string(' ', _indentLevel * 4);

    private void LogParse(string message)
    {
        if (_silentMode) return;
        if (_debugMode)
        {
            Console.WriteLine($"({O.GetPrimaryOffset()},{O.GetSecondaryOffset()}) {Indent()}{message}");
        }
        else
        {
            Console.WriteLine($"{Indent()}{message}");
        }
    }

    public bool Parse()
    {
        try
        {
            var fopts = new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Share = FileShare.Read,
                Options = FileOptions.SequentialScan
            };
            _fs = new FileStream(_filename, fopts);
            _offset = new OffsetManager(_fs);

            var ext = Path.GetExtension(_filename);
            var fileType = _catalog.GetFileType(ext) ?? _catalog.GetFileTypeByName(_filename);
            if (fileType is null) return false;

            var versioned = _catalog.GetVersionedFileTypeInfo(fileType);
            List<string> structTypes;
            int secStart;
            if (versioned is not null)
            {
                structTypes = versioned.StructTypes;
                secStart = versioned.SecondaryOffsetStart;
            }
            else
            {
                structTypes = fileType.StructTypes;
                secStart = fileType.SecondaryOffsetStart;
            }

            O.SetPrimaryOffset(0);
            O.SetSecondaryOffset(secStart);

            if (_exportMode && _exporter is not null) _exporter.BeginDocument();

            foreach (var st in structTypes)
            {
                _isProcessingRootTag = true;
                ParseStruct(st);
                _isProcessingRootTag = false;
            }

            if (_exportMode && _exporter is not null) _exporter.EndDocument();

            _fs.Dispose();
            _fs = null;
            _offset = null;
            return true;
        }
        catch
        {
            try { _fs?.Dispose(); } catch {}
            _fs = null;
            _offset = null;
            return false;
        }
    }

    public void ExportToFile(string outputFile)
    {
        if (_exportMode && _exporter is not null) _exporter.SaveToFile(outputFile);
    }

    private void ParseStruct(string structName, int arrayIndex = -1)
    {
        try
        {
            var structDef = _catalog.GetStruct(structName);
            if (structDef is null) return;

            if (arrayIndex >= 0) LogParse($"parse_struct({structName}, [{arrayIndex}])");
            else LogParse($"parse_struct({structName})");

            var shouldEndNode = false;
            if (_exportMode && _exporter is not null)
            {
                if (_isProcessingRootTag)
                {
                    var lower = structName.ToLowerInvariant();
                    _exporter.BeginNode(lower);
                    _isProcessingRootTag = false;
                    shouldEndNode = true;
                }
                else if (arrayIndex < 0 && !_isInsideNullable)
                {
                    _exporter.BeginNode(structName);
                    shouldEndNode = true;
                }
            }

            var previousStructBaseOffset = _currentStructBaseOffset;
            var structBaseOffset = O.GetPrimaryOffset();

            if (_secOffsetStruct)
            {
                _structBaseOffsetStack.Push(previousStructBaseOffset);
                _currentStructBaseOffset = O.GetSecondaryOffset();
                if (!_processingArrayElement)
                {
                    O.SetSecondaryOffset(O.GetSecondaryOffset() + structDef.FixedSize);
                }
            }

            _indentLevel++;

            var structStartOffset = O.GetPrimaryOffset();
            foreach (var member in structDef.Members)
            {
                if (_processingArrayElement) O.SetPrimaryOffset(structStartOffset);
                ParseMember(member, structDef);
            }

            _indentLevel--;
            if (_secOffsetStruct)
            {
                if (_structBaseOffsetStack.Count > 0)
                {
                    _currentStructBaseOffset = _structBaseOffsetStack.Pop();
                }
                else
                {
                    _currentStructBaseOffset = 0;
                }
            }
            else
            {
                _currentStructBaseOffset = previousStructBaseOffset;
            }

            if (_exportMode && _exporter is not null && shouldEndNode) _exporter.EndNode();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error in parse_struct({structName}): {e.Message} at position ({O.GetPrimaryOffset()}, {O.GetSecondaryOffset()})");
        }
    }

    private void ParseMember(StructMember member, StructDefinition parentStruct, int arraySize = 0)
    {
        var typeDef = _catalog.GetType(member.TypeName);
        if (typeDef is null) return;

        var originalSecondaryOffset = O.GetRealSecondaryOffset();
        var arrayStructOffset = O.GetPrimaryOffset();

        if (member.TypeName == "array")
        {
            var structStartOffset = _currentStructBaseOffset;

            long arrayStartOffset;
            if (_secOffsetStruct)
            {
                if (_processingArrayElement) arrayStartOffset = arrayStructOffset + member.Offset;
                else if (_isInsideNullable) arrayStartOffset = _startNullableOffset + member.Offset;
                else arrayStartOffset = structStartOffset + member.Offset;
            }
            else if (member.UseSecondaryOffset)
            {
                arrayStartOffset = member.Offset;
            }
            else
            {
                arrayStartOffset = _currentStructBaseOffset + member.Offset;
            }

            O.SetPrimaryOffset(arrayStartOffset);
            var hasValue = O.ReadPrimary<uint>();

            var useSecondaryForElements = _secOffsetStruct || (!_secOffsetStruct && !_processingArrayElement);
            var arrayDataOffset = useSecondaryForElements ? O.GetSecondaryOffset() : O.GetPrimaryOffset();

            if (hasValue != 0)
            {
                uint count;
                if (member.CountOffset > 0)
                {
                    var countOffset = _startNullableOffset + member.Offset + member.CountOffset;
                    count = O.ReadAt<uint>(countOffset);
                }
                else
                {
                    count = O.ReadPrimary<uint>();
                }

                var elemTypeDef = _catalog.GetType(member.ElementType);
                var elemStructDef = _catalog.GetStruct(member.ElementType);
                LogParse($"parse_member_array({member.Name}, {count})");
                _indentLevel++;

                if (_exportMode && _exporter is not null) _exporter.BeginArray(member.Name);

                if (elemStructDef is not null)
                {
                    var elementSize = elemStructDef.FixedSize;
                    var elementBaseOffset = O.GetPrimaryOffset();

                    _totalArraySize = elemStructDef.FixedSize * count;
                    O.SetSecondaryOffset(originalSecondaryOffset + _totalArraySize);

                    for (var i = 0u; i < count; i++)
                    {
                        if (_exportMode && _exporter is not null) _exporter.BeginArrayEntry();

                        if (useSecondaryForElements)
                        {
                            O.SetPrimaryOffset(arrayDataOffset);
                            var oldSec = _secOffsetStruct;
                            _secOffsetStruct = true;
                            _processingArrayElement = true;
                            ParseStruct(member.ElementType, (int)i);
                            _secOffsetStruct = oldSec;
                            arrayDataOffset += elemStructDef.FixedSize;
                        }
                        else
                        {
                            O.SetPrimaryOffset(elementBaseOffset);
                            _processingArrayElement = true;
                            ParseStruct(member.ElementType, (int)i);
                            elementBaseOffset += elementSize;
                        }

                        if (_exportMode && _exporter is not null) _exporter.EndArrayEntry();
                    }
                }
                else
                {
                    var useSecondaryForElements2 = !_secOffsetStruct && !_processingArrayElement;
                    var elementBaseOffset = useSecondaryForElements2 ? O.GetSecondaryOffset() : O.GetPrimaryOffset();

                    var totalElementSize = count * (uint)(elemTypeDef?.Size ?? 0);
                    if (useSecondaryForElements2) O.SetSecondaryOffset(originalSecondaryOffset + totalElementSize);

                    for (var i = 0; i < count; i++)
                    {
                        var elementSize = elemTypeDef?.Size ?? 0;

                        if (useSecondaryForElements2)
                        {
                            O.SetPrimaryOffset(elementBaseOffset);
                            elementBaseOffset += elementSize;
                        }

                        var elementMember = new StructMember("entry", member.ElementType, O.GetPrimaryOffset() <= int.MaxValue ? (int)O.GetPrimaryOffset() : 0, useSecondaryForElements2);

                        if (_exportMode && _exporter is not null) _exporter.BeginArrayEntry();

                        ParseMember(elementMember, parentStruct);

                        if (_exportMode && _exporter is not null) _exporter.EndArrayEntry();

                        if (!useSecondaryForElements2) O.AdvancePrimary(elementSize);
                    }
                }

                if (_exportMode && _exporter is not null) _exporter.EndArray();

                _processingArrayElement = false;
                _indentLevel--;
            }

            return;
        }

        if (_secOffsetStruct)
        {
            if (_processingArrayElement) O.SetPrimaryOffset(arrayStructOffset + member.Offset);
            else O.SetPrimaryOffset(_currentStructBaseOffset + member.Offset);
        }
        else if (member.UseSecondaryOffset)
        {
            O.SetPrimaryOffset(member.Offset);
        }
        else
        {
            O.SetPrimaryOffset(_currentStructBaseOffset + member.Offset);
        }

        var isSpecial = typeDef.Type == DataType.KEY || typeDef.Type == DataType.ASSET || typeDef.Type == DataType.NULLABLE || typeDef.Type == DataType.CHAR_PTR;

        string valueStr = string.Empty;
        string logMessage = string.Empty;

        switch (typeDef.Type)
        {
            case DataType.BOOL:
            {
                var v = O.ReadPrimary<bool>();
                valueStr = v ? "true" : "false";
                logMessage = $"parse_member_bool({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportBool(member.Name, v);
                break;
            }
            case DataType.INT:
            {
                var v = O.ReadPrimary<int>();
                valueStr = v.ToString(CultureInfo.InvariantCulture);
                logMessage = $"parse_member_int({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportInt(member.Name, v);
                break;
            }
            case DataType.FLOAT:
            {
                var v = O.ReadPrimary<float>();
                valueStr = v.ToString("0.00000", CultureInfo.InvariantCulture);
                logMessage = $"parse_member_float({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportFloat(member.Name, v);
                break;
            }
            case DataType.GUID:
            {
                var d1 = O.ReadPrimary<uint>();
                var d2 = O.ReadPrimary<ushort>();
                var d3 = O.ReadPrimary<ushort>();
                var d4 = O.ReadPrimary<ulong>();
                var part4a = (d4 >> 48) & 0xFFFF;
                var part4b = d4 & 0xFFFFFFFFFFFFUL;
                valueStr = $"{d1:x8}-{d2:x4}-{d3:x4}-{part4a:x4}-{part4b:x12}";
                logMessage = $"parse_member_guid({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportGuid(member.Name, valueStr);
                break;
            }
            case DataType.VECTOR2:
            {
                var x = O.ReadPrimary<float>();
                var y = O.ReadPrimary<float>();
                valueStr = $"x: {x.ToString("0.00000", CultureInfo.InvariantCulture)}, y: {y.ToString("0.00000", CultureInfo.InvariantCulture)}";
                logMessage = $"parse_member_cSPVector2({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportVector2(member.Name, x, y);
                break;
            }
            case DataType.VECTOR3:
            {
                var x = O.ReadPrimary<float>();
                var y = O.ReadPrimary<float>();
                var z = O.ReadPrimary<float>();
                valueStr = $"x: {x.ToString("0.00000", CultureInfo.InvariantCulture)}, y: {y.ToString("0.00000", CultureInfo.InvariantCulture)}, z: {z.ToString("0.00000", CultureInfo.InvariantCulture)}";
                logMessage = $"parse_member_cSPVector3({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportVector3(member.Name, x, y, z);
                break;
            }
            case DataType.QUATERNION:
            {
                var w = O.ReadPrimary<float>();
                var x = O.ReadPrimary<float>();
                var y = O.ReadPrimary<float>();
                var z = O.ReadPrimary<float>();
                valueStr = $"w: {w.ToString("0.00000", CultureInfo.InvariantCulture)}, x: {x.ToString("0.00000", CultureInfo.InvariantCulture)}, y: {y.ToString("0.00000", CultureInfo.InvariantCulture)}, z: {z.ToString("0.00000", CultureInfo.InvariantCulture)}";
                logMessage = $"parse_member_cSPVector4({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportQuaternion(member.Name, w, x, y, z);
                break;
            }
            case DataType.KEY:
            {
                var off = O.ReadPrimary<uint>();
                if (off != 0)
                {
                    var s = O.ReadString(true);
                    valueStr = s;
                    logMessage = $"parse_member_key({member.Name}, {valueStr})";
                    if (_exportMode && _exporter is not null) _exporter.ExportString(member.Name, valueStr);
                }
                else
                {
                    return;
                }
                break;
            }
            case DataType.CKEYASSET:
            {
                var off = O.ReadPrimary<uint>();
                if (off != 0)
                {
                    var s = O.ReadString(true);
                    valueStr = s;
                    logMessage = $"parse_member_cKeyAsset({member.Name}, {valueStr})";
                    if (_exportMode && _exporter is not null) _exporter.ExportString(member.Name, valueStr);
                }
                else
                {
                    return;
                }
                break;
            }
            case DataType.LOCALIZEDASSETSTRING:
            {
                var off = O.ReadPrimary<uint>();
                var assetStr = O.ReadPrimary<uint>();
                if (off != 0)
                {
                    var str = O.ReadString(true);
                    if (assetStr != 0)
                    {
                        var id = O.ReadString(true);
                        logMessage = $"parse_member_cLocalizedAssetString({member.Name}, {str}, {id})";
                        if (_exportMode && _exporter is not null)
                        {
                            _exporter.BeginNode(member.Name);
                            _exporter.ExportString("text", str);
                            _exporter.ExportString("id", id);
                            _exporter.EndNode();
                        }
                    }
                    else
                    {
                        logMessage = $"parse_member_cLocalizedAssetString({member.Name}, {str})";
                        if (_exportMode && _exporter is not null) _exporter.ExportString(member.Name, str);
                    }
                }
                else
                {
                    return;
                }
                break;
            }
            case DataType.ASSET:
            {
                var off = O.ReadPrimary<uint>();
                if (off > 0)
                {
                    var s = O.ReadString(true);
                    valueStr = s;
                    logMessage = $"parse_member_asset({member.Name}, {valueStr})";
                    if (_exportMode && _exporter is not null) _exporter.ExportString(member.Name, valueStr);
                }
                else
                {
                    return;
                }
                break;
            }
            case DataType.CHAR_PTR:
            {
                var off = O.ReadPrimary<uint>();
                if (off > 0)
                {
                    var s = O.ReadString(true);
                    valueStr = s;
                    logMessage = $"parse_member_char*({member.Name}, {valueStr})";
                    if (_exportMode && _exporter is not null) _exporter.ExportString(member.Name, valueStr);
                }
                else
                {
                    return;
                }
                break;
            }
            case DataType.CHAR:
            {
                var s = O.ReadString();
                valueStr = s;
                if (!string.IsNullOrEmpty(s) && s != "0")
                {
                    logMessage = $"parse_member_char({member.Name}, {valueStr})";
                    if (_exportMode && _exporter is not null) _exporter.ExportString(member.Name, valueStr);
                }
                else
                {
                    return;
                }
                break;
            }
            case DataType.ENUM:
            {
                var v = O.ReadPrimary<uint>();
                valueStr = v.ToString(CultureInfo.InvariantCulture);
                logMessage = $"parse_member_enum({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportUInt32(member.Name, v);
                break;
            }
            case DataType.UINT8:
            {
                var v = O.ReadPrimary<byte>();
                valueStr = v.ToString(CultureInfo.InvariantCulture);
                logMessage = $"parse_member_uint8_t({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportUInt8(member.Name, v);
                break;
            }
            case DataType.UINT16:
            {
                var v = O.ReadPrimary<ushort>();
                valueStr = v.ToString(CultureInfo.InvariantCulture);
                logMessage = $"parse_member_uint16_t({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportUInt16(member.Name, v);
                break;
            }
            case DataType.UINT32:
            {
                var v = O.ReadPrimary<uint>();
                valueStr = v.ToString(CultureInfo.InvariantCulture);
                logMessage = $"parse_member_uint32_t({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportUInt32(member.Name, v);
                break;
            }
            case DataType.UINT64:
            {
                var v = O.ReadPrimary<ulong>();
                valueStr = "0x" + v.ToString("X", CultureInfo.InvariantCulture);
                logMessage = $"parse_member_uint64_t({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportUInt64(member.Name, v);
                break;
            }
            case DataType.INT64:
            {
                var v = O.ReadPrimary<long>();
                var u = unchecked((ulong)v);
                valueStr = "0x" + u.ToString("X", CultureInfo.InvariantCulture);
                logMessage = $"parse_member_int64_t({member.Name}, {valueStr})";
                if (_exportMode && _exporter is not null) _exporter.ExportInt64(member.Name, v);
                break;
            }
            case DataType.NULLABLE:
            {
                var startOffset = O.GetPrimaryOffset();
                var hasValue = O.ReadPrimary<uint>();
                if (hasValue > 0 && !string.IsNullOrEmpty(typeDef.TargetType))
                {
                    var targetStruct = _catalog.GetStruct(typeDef.TargetType);
                    if (targetStruct is not null)
                    {
                        if (member.HasCustomName) LogParse($"parse_member_nullable({member.Name}, {typeDef.TargetType})");
                        else LogParse($"parse_member_nullable({typeDef.TargetType})");

                        _startNullableOffset = O.GetRealSecondaryOffset();

                        var oldSec = _secOffsetStruct;
                        var oldBase = _currentStructBaseOffset;
                        var oldArray = _processingArrayElement;

                        _secOffsetStruct = true;
                        _processingArrayElement = true;
                        _isInsideNullable = true;

                        O.SetPrimaryOffset(O.GetSecondaryOffset());
                        O.SetSecondaryOffset(originalSecondaryOffset + targetStruct.FixedSize);

                        if (_exportMode && _exporter is not null)
                        {
                            _exporter.BeginNode(member.Name);
                            ParseStruct(typeDef.TargetType);
                            _exporter.EndNode();
                        }
                        else
                        {
                            ParseStruct(typeDef.TargetType);
                        }

                        _processingArrayElement = oldArray;
                        _secOffsetStruct = oldSec;
                        _currentStructBaseOffset = oldBase;
                        _isInsideNullable = false;

                        O.SetPrimaryOffset(startOffset + 4);
                        return;
                    }
                }
                else
                {
                    O.SetPrimaryOffset(startOffset + 4);
                    return;
                }
                break;
            }
            case DataType.STRUCT:
            {
                var target = typeDef.TargetType;
                if (member.HasCustomName) logMessage = $"parse_member_struct({member.Name}, {target})";
                else logMessage = $"parse_member_struct({target})";
                LogParse(logMessage);
                var current = O.GetPrimaryOffset();
                var prevBase = _currentStructBaseOffset;
                _currentStructBaseOffset = current;

                if (_exportMode && _exporter is not null && member.HasCustomName)
                {
                    _exporter.BeginNode(member.Name);
                    ParseStruct(target);
                    _exporter.EndNode();
                }
                else
                {
                    ParseStruct(target);
                }

                _currentStructBaseOffset = prevBase;
                return;
            }
            default:
            {
                valueStr = "unknown";
                logMessage = $"parse_member_unknown({member.Name}, {valueStr})";
                break;
            }
        }

        LogParse(logMessage);
    }
}
