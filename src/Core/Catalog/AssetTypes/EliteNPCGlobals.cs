namespace ReCap.Parser.Catalog;

public sealed class EliteNPCGlobals : AssetCatalog
{
    protected override void Build()
    {
        Struct("EliteNPCGlobals", 0x18,
            ArrayStruct("perLevelTuning", "cAffixDifficultyTuning", 0x0),
            Field("textColor", DataType.Vector4, 0x8)
        );
    }
}