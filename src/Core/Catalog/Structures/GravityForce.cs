namespace ReCap.Parser.Catalog;

public sealed class GravityForce : AssetCatalog
{
    protected override void Build()
    {
        Struct("GravityForce", 0xc,
            Field("radius", DataType.Float, 0x8),
            Field("force", DataType.Float, 0xc),
            Field("forceForMover", DataType.Float, 0x10)
        );
    }
}