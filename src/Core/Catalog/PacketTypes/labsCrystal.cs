namespace ReCap.Parser.Catalog;

public sealed class labsCrystal : AssetCatalog
{
    protected override void Build()
    {
        Struct("labsCrystal", 0x10,
            Field("mLabsCrystal.crystalNoun", DataType.Asset, 0x0),
            Field("mLabsCrystal.level", DataType.UInt16, 0x4)
        );
    }   
}
