namespace ReCap.Parser.Catalog;

public sealed class cSpaceshipCameraTuning : AssetCatalog
{
    protected override void Build()
    {
        Struct("cSpaceshipCameraTuning", 0x14,
            Field("name", DataType.Key, 0xc),
            Field("weight", DataType.Float, 0x10)
        );
    }
}