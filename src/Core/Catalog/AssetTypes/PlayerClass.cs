namespace ReCap.Parser.Catalog;

public sealed class PlayerClass : AssetCatalog
{
    protected override void Build()
    {
        Struct("PlayerClass", 0x100,
            Field("testingOnly", DataType.Bool, 0x0),
            Field("speciesName", DataType.CharPtr, 0x10),
            Field("nameLocaleKey", DataType.Key, 0x20),
            Field("shortNameLocaleKey", DataType.Key, 0x30),
            Field("localeTableID", DataType.Key, 0x40),
            Field("homeworld", DataType.Bool, 0x44),
            Field("creatureClass", DataType.Enum, 0x48),
            Field("primaryAttribute", DataType.Enum, 0x4c),
            Field("unlockLevel", DataType.Int, 0x50),
            Field("basicAbility", DataType.Key, 0x60),
            Field("specialAbility1", DataType.Key, 0x70),
            Field("specialAbility2", DataType.Key, 0x80),
            Field("specialAbility3", DataType.Key, 0x90),
            Field("passiveAbility", DataType.Key, 0xa0),
            Field("sharedAbilityOffset", DataType.Vector3, 0xac),
            Field("mpClassAttributes", DataType.Asset, 0xc),
            Field("mpClassEffect", DataType.Asset, 0x8),
            Field("originalHandBlock", DataType.Key, 0xc4),
            Field("originalFootBlock", DataType.Key, 0xd4),
            Field("originalWeaponBlock", DataType.Key, 0xe4),
            Field("weaponMinDamage", DataType.Float, 0xe8),
            Field("weaponMaxDamage", DataType.Float, 0xec),
            Field("noHands", DataType.Bool, 0xf8),
            Field("noFeet", DataType.Bool, 0xf9),
            Array("descriptionTag", DataType.Enum, 0xa4),
            ArrayStruct("editableCharacterPart", "cKeyAsset", 0xf0)
        );
    }
}
