namespace ReCap.Parser.Catalog;

public sealed class Markerset : AssetCatalog
{
    protected override void Build()
    {
        Struct("Markerset", 0x28,
            ArrayStruct("markers", "cLabsMarker", 0x0),
            Field("group", DataType.Key, 0x14),
            Field("weight", DataType.Float, 0x18),
            Array("condition", DataType.Enum, 0x20)
        );
    }
}
