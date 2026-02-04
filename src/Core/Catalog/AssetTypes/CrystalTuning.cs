namespace ReCap.Parser.Catalog;

public sealed class CrystalTuning : AssetCatalog
{
    protected override void Build()
    {
        Struct("CrystalTuning", 0x14,
            Field("threeInARowBonusPercent", DataType.Float, 0x0),
            ArrayStruct("CrystalLevel", "CrystalLevel", 0xc),
            ArrayStruct("crystal", "CrystalDropDef", 0x4)
        );
    }
}