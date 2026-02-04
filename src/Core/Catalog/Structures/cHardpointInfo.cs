namespace ReCap.Parser.Catalog;

public sealed class cHardpointInfo : AssetCatalog
{
    protected override void Build()
    {
        Struct("cHardpointInfo", 0x18,
            Field("type", DataType.Enum, 0x0),
            Field("bodyCap", DataType.Enum, 0x4),
            Field("localOffset", DataType.Vector3, 0x8),
            Field("attractor", DataType.Bool, 0x14),
            Field("attached", DataType.Bool, 0x15)
        );
    }
}
