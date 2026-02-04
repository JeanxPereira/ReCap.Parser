namespace ReCap.Parser.Catalog;

public sealed class Gfx : AssetCatalog
{
    protected override void Build()
    {
        Struct("Gfx", 0xc,
            ArrayStruct("components", "cGfxComponentDef", 0x0),
            Field("boundingRadius", DataType.Float, 0x8)
        );
    }
}