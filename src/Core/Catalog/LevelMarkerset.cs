namespace ReCap.Parser.Catalog;

public sealed class LevelMarkerset : AssetCatalog
{
    protected override void Build()
    {
        Struct("LevelMarkerset", 0x4,
            Field("markersetAsset", DataType.Asset, 0x0)
        );
    }
}
