namespace ReCap.Parser.Catalog;

public sealed class LevelKey : AssetCatalog
{
    protected override void Build()
    {
        Struct("LevelKey", 0x10,
            Field("key", DataType.Key, 0xc)
        );
    }
}
