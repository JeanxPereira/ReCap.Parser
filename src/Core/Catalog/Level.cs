namespace ReCap.Parser.Catalog;

public sealed class Level : AssetCatalog
{
    protected override void Build()
    {
        Struct("Level", 0x98,
            ArrayStruct("markersets", "LevelMarkerset", 0x0),
            Field("music", DataType.Key, 0x14),
            Field("navMesh", DataType.Key, 0x24),
            Field("physicsMesh", DataType.Key, 0x34),
            Field("renderingConfig", DataType.Key, 0x44),
            Field("footstepEffect", DataType.Key, 0x54),
            NStruct("levelConfig", "LevelConfig", 0x58), 
            NStruct("firstTimeConfig", "LevelConfig", 0x5c),
            Field("planetConfig", DataType.Key, 0x60),
            Field("levelObjectives", DataType.Asset, 0x64),
            Field("name", DataType.UInt32, 0x68),
            Field("briefDescription", DataType.UInt32, 0x6c),
            Field("description", DataType.UInt32, 0x70),
            EnumField("primaryType", "LevelType", 0x74),
            EnumField("secondaryType", "LevelType", 0x78),
            EnumField("tertiaryType", "LevelType", 0x7c),
            EnumField("quadernaryType", "LevelType", 0x80),
            Field("planetScreenBG", DataType.Key, 0x90),
            NStruct("cameraSettings", "LevelCameraSettings", 0x94)
        );
    }
}