namespace ReCap.Parser.Catalog;

public sealed class LevelObjectives : AssetCatalog
{
    protected override void Build()
    {
        Struct("LevelObjectives", 0x28,
            ArrayStruct("objective", "LevelKey", 0x10),
            ArrayStruct("affix", "LevelKey", 0x8),
            ArrayStruct("positiveAffix", "LevelKey", 0x10),
            ArrayStruct("minorAffix", "LevelKey", 0x18),
            ArrayStruct("majorAffix", "LevelKey", 0x20)
        );
    }
}
