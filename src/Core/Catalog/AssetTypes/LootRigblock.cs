namespace ReCap.Parser.Catalog;

public sealed class LootRigblock : AssetCatalog
{
    protected override void Build()
    {
        Struct("LootRigblock", 0x8c,
            Field("rigblockId", DataType.UInt32, 0x0),
            Field("rigblockName", DataType.Key, 0x10),
            Field("rigblockPropKey", DataType.Key, 0x20),
            Field("rigblockPartType", DataType.CharPtr, 0x24),
            ArrayStruct("classTypes", "LootData", 0x28),
            ArrayStruct("scienceTypes", "LootData", 0x30),
            ArrayStruct("playerCharacters", "LootData", 0x38),
            Field("minLevel", DataType.Int, 0x40),
            Field("maxLevel", DataType.Int, 0x44),
            Field("rigblockPngKey", DataType.Key, 0x54),
            Field("rigblockCategoryKey", DataType.Key, 0x64),
            Field("isUnique", DataType.Bool, 0x68)
        );
    }
}
