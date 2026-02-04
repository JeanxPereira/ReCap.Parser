namespace ReCap.Parser.Catalog;

public sealed class LootData : AssetCatalog
{
    protected override void Build()
    {
        Struct("LootData", 0x4,
            Field("name", DataType.CharPtr, 0x0)
        );
    }
}