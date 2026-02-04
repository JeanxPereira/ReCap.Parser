namespace ReCap.Parser.Catalog;

public sealed class DirectorTuning : AssetCatalog
{
    protected override void Build()
    {
        Struct("DirectorTuning", 0x10,
            Array("OrbDifficultyScale", DataType.Float, 0x0),
            Array("HordeDifficultyWave", DataType.Int, 0x8)
        );
    }
}