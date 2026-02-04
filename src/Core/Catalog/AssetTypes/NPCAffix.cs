namespace ReCap.Parser.Catalog;

public sealed class NPCAffix : AssetCatalog
{
    protected override void Build()
    {
        Struct("NPCAffix", 0x2c,
            Field("modifier", DataType.Key, 0xc),
            Field("mpChildAffix", DataType.Asset, 0x10),
            Field("mpParentAffix", DataType.Asset, 0x14),
            Field("description", DataType.cLocalizedAssetString, 0x18)
        );
    }
}