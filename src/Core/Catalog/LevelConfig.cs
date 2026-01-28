namespace ReCap.Parser.Catalog;

public sealed class LevelConfig : AssetCatalog
{
    protected override void Build()
    {
        Struct("LevelConfig", 0x28,
            ArrayStruct("minion", "DirectorClass", 0x0, countOffset: 0x14),
            ArrayStruct("special", "DirectorClass", 0x4, countOffset: 0x14),
            ArrayStruct("boss", "DirectorClass", 0x8, countOffset: 0x14),
            ArrayStruct("agent", "DirectorClass", 0xc, countOffset: 0x14),
            ArrayStruct("captain", "DirectorClass", 0x10)
        );
    }
}