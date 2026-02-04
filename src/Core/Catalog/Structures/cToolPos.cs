namespace ReCap.Parser.Catalog;

public sealed class cToolPos : AssetCatalog
{
    protected override void Build()
    {
        Struct("cToolPos", 0x14,
            Field("visible", DataType.Bool, 0x0),
            Field("left", DataType.Int, 0x4),
            Field("right", DataType.Int, 0x8),
            Field("top", DataType.Int, 0xc),
            Field("bottom", DataType.Int, 0x10)
        );
    }
}