namespace ReCap.Parser.Catalog;

public sealed class SectionConfig : AssetCatalog
{
    protected override void Build()
    {
        Struct("SectionConfig", 0x8,
            ArrayStruct("bucket", "DirectorBucket", 0x0)
        );
    }
}