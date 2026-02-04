namespace ReCap.Parser.Catalog;

public sealed class ExtentsCategory : AssetCatalog
{
    protected override void Build()
    {
        Struct("ExtentsCategory", 0x10,
            Field("extents", DataType.Vector3, 0x0),
            Field("graphicsScale", DataType.Float, 0xc)
        );
    }
}