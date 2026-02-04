namespace ReCap.Parser.Catalog;

public sealed class UnlocksTuning : AssetCatalog
{
    protected override void Build()
    {
        Struct("UnlocksTuning", 0x8,
            ArrayStruct("unlock", "UnlockDef", 0x0)
        );
    }
}
