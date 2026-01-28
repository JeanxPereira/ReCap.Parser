namespace ReCap.Parser.Catalog;

public sealed class DirectorClass : AssetCatalog
{
    protected override void Build()
    {
        Struct("DirectorClass", 0x10,
            Field("mpNoun", DataType.Asset, 0x0),
            Field("minDifficulty", DataType.Int, 0x4),
            Field("maxDifficulty", DataType.Int, 0x8),
            Field("hordeLegal", DataType.Bool, 0xc)
        );
    }
}