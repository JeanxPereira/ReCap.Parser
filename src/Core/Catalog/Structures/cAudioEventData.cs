namespace ReCap.Parser.Catalog;

public sealed class cAudioEventData : AssetCatalog
{
    protected override void Build()
    {
        Struct("cAudioEventData", 0x20,
            Field("sound", DataType.Key, 0xc),
            Field("bIs3D", DataType.Bool, 0x10),
            Field("bIsVoiceOver", DataType.Bool, 0x12),
            Field("bHasLocalOffset", DataType.Bool, 0x11),
            Field("localOffset", DataType.Vector3, 0x14)
        );
    }
}