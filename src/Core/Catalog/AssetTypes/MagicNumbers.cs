namespace ReCap.Parser.Catalog;

public sealed class MagicNumbers : AssetCatalog
{
    protected override void Build()
    {
        Struct("MagicNumbers", 0x4c,
            Field("DamagePerPointOfStrength", DataType.Float, 0x0),
            Field("DamagePerPointOfDexterity", DataType.Float, 0x4),
            Field("DamagePerPointOfMind", DataType.Float, 0x8),
            Field("HealthPerPointofStrength", DataType.Float, 0xc),
            Field("PhysicalDefensePerPointofDexterity", DataType.Float, 0x10),
            Field("CritRatingPerPointofDexterity", DataType.Float, 0x14),
            Field("EnergyDefensePerPointofMind", DataType.Float, 0x18),
            Field("ManaPerPointofMind", DataType.Float, 0x1c),
            Field("DefenseRatingDecreaseMultiplier", DataType.Float, 0x20),
            Field("DefenseRatingDecreaseBase", DataType.Float, 0x24),
            Field("CriticalRatingDecreaseMultiplier", DataType.Float, 0x28),
            Field("CriticalRatingDecreaseBase", DataType.Float, 0x2c),
            Field("CriticalDamageBonus", DataType.Float, 0x30),
            Field("PrimaryAttributeIgnoreAmount", DataType.Int, 0x34),
            Field("LeechEffectivenessForAoE", DataType.Float, 0x38),
            Field("LeechEffectivenessForAbilities", DataType.Float, 0x3c),
            Field("LeechEffectivenessForBasics", DataType.Float, 0x40),
            Field("DodgePercentCap", DataType.Float, 0x44),
            Field("ResistPercentCap", DataType.Float, 0x48)
        );
    }
}
