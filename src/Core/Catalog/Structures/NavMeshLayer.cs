namespace ReCap.Parser.Catalog;

public sealed class NavMeshLayer : AssetCatalog
{
    protected override void Build()
    {
        Struct("NavMeshLayer", 0x18,
            Field("name", DataType.CharPtr, 0x0),
            Field("voxelTestSize", DataType.Float, 0x4),
            Field("agentRadius", DataType.Float, 0x8),
            Field("agentHeight", DataType.Float, 0xc),
            Field("maxStepSize", DataType.Float, 0x10),
            Field("maxWalkableSlopeDegrees", DataType.Float, 0x14)
        );
    }
}