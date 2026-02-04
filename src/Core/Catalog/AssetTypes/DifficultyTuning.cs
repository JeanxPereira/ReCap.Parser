namespace ReCap.Parser.Catalog;

public sealed class DifficultyTuning : AssetCatalog
{
    protected override void Build()
    {
        Struct("DifficultyTuning", 0x48,
            Array("HealthPercentIncrease", DataType.Float, 0x0),
            Array("DamagePercentIncrease", DataType.Float, 0x8),
            Array("ItemLevelRange", DataType.Vector2, 0x10),
            Array("GearScoreRange", DataType.Vector2, 0x18),
            Field("GearScoreMax", DataType.Vector2, 0x20),
            Array("ExpectedAvatarLevel", DataType.Int, 0x28),
            Array("RatingConversion", DataType.Float, 0x30),
            Field("StarModeHealthMult", DataType.Float, 0x38),
            Field("StarModeDamageMult", DataType.Float, 0x3c),
            Field("StarModeEliteChanceAdd", DataType.Float, 0x40),
            Field("StarModeSuggestedLevelAdd", DataType.Float, 0x44)
        );
    }
}