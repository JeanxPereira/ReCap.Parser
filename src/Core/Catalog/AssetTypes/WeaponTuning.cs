namespace ReCap.Parser.Catalog;

public sealed class WeaponTuning : AssetCatalog
{
    protected override void Build()
    {
        Struct("WeaponTuning", 0x8,
            ArrayStruct("weapon", "WeaponDef", 0x0)
        );
    }
}
