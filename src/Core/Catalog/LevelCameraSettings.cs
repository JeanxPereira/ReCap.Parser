namespace ReCap.Parser.Catalog;

public sealed class LevelCameraSettings : AssetCatalog
{
    protected override void Build()
    {
        Struct("LevelCameraSettings", 0xc,
            Field("pitchOverride", DataType.Float, 0x0),
            Field("yawOverride", DataType.Float, 0x4),
            Field("distanceOverride", DataType.Float, 0x8)
        );
    }
}
