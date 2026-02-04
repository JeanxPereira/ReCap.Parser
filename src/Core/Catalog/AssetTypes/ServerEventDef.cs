namespace ReCap.Parser.Catalog;

public sealed class ServerEventDef : AssetCatalog
{
    protected override void Build()
    {
        Struct("ServerEventDef", 0x10,
            ArrayStruct("audio", "cAudioEventData", 0x0),
            ArrayStruct("effects", "cEffectEventData", 0x8)
        );
    }
}
