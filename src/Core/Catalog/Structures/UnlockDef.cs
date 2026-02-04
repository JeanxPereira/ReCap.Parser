namespace ReCap.Parser.Catalog;

public sealed class UnlockDef : AssetCatalog
{
    protected override void Build()
    {
        Struct("UnlockDef", 0x50,
            Field("id", DataType.Int, 0x0),
            Field("prerequisite", DataType.Int, 0x4),
            Field("cost", DataType.Int, 0x8),
            Field("level", DataType.Int, 0xc),
            Field("rank", DataType.Int, 0x10),
            Field("unlockType", DataType.Enum, 0x14),
            Field("value", DataType.Int, 0x18),
            Field("unlockFunction", DataType.Enum, 0x1c),
            Field("image", DataType.Key, 0x2c),
            Field("title", DataType.Key, 0x3c),
            Field("description", DataType.Key, 0x4c)
        );
    }
}
