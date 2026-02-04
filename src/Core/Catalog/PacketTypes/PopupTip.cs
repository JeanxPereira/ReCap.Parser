namespace ReCap.Parser.Catalog;

public sealed class PopupTip : AssetCatalog
{
    protected override void Build()
    {
        Struct("PopupTip", 0x68,
            Field("namespace", DataType.Char, 0x14, 0x50)
        );
    }   
}
