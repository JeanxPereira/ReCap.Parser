namespace ReCap.Parser.Catalog;

public sealed class NavPowerTuning : AssetCatalog
{
    protected override void Build()
    {
        Struct("NavPowerTuning", 0xc,
            Field("maxWalkableSlopeDegrees", DataType.Float, 0x0),
            ArrayStruct("navMeshLayers", "NavMeshLayer", 0x4)
        );
    }
}