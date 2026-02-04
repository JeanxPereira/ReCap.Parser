namespace ReCap.Parser.Catalog;

public sealed class WeaponDef : AssetCatalog
{
    protected override void Build()
    {
        Struct("WeaponDef", 0x20,
            Field("refId", DataType.UInt64, 0x0),
            Field("itemLevel", DataType.Int, 0x8),
            Field("rigblockId", DataType.UInt32, 0xc),
            Field("suffixId", DataType.UInt32, 0x10),
            Field("avatarLevel", DataType.Int, 0x18),
            Field("chainProgression", DataType.Enum, 0x1c)
        );
    }
}
