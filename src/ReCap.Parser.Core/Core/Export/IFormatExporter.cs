namespace ReCap.Parser.Core.Core;

public interface IFormatExporter
{
    void BeginDocument();
    void EndDocument();
    void BeginNode(string name);
    void EndNode();
    void ExportBool(string name, bool value);
    void ExportInt(string name, int value);
    void ExportUInt8(string name, byte value);
    void ExportUInt16(string name, ushort value);
    void ExportUInt32(string name, uint value);
    void ExportUInt64(string name, ulong value);
    void ExportInt64(string name, long value);
    void ExportFloat(string name, float value);
    void ExportString(string name, string value);
    void ExportGuid(string name, string value);
    void ExportVector2(string name, float x, float y);
    void ExportVector3(string name, float x, float y, float z);
    void ExportQuaternion(string name, float w, float x, float y, float z);
    void BeginArray(string name);
    void BeginArrayEntry();
    void EndArrayEntry();
    void EndArray();
    bool SaveToFile(string filepath);
}
