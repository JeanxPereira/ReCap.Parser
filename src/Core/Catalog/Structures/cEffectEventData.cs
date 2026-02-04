namespace ReCap.Parser.Catalog;

public sealed class cEffectEventData : AssetCatalog
{
    protected override void Build()
    {
        Struct("cEffectEventData", 0x40,
            Field("name", DataType.Key, 0xc),
            Field("bScaleWithObject", DataType.Bool, 0x10),
            Field("localizedTextKey", DataType.Key, 0x18),
            Field("bHasTextValue", DataType.Bool, 0x11),
            Field("bSetModelPointer", DataType.Bool, 0x12),
            Field("bZUpAlignment", DataType.Bool, 0x13),
            Field("bCreatureOrientationAlignment", DataType.Bool, 0x14),
            Field("bUseTargetPoint", DataType.Bool, 0x15),
            Field("bSetDirectionFromSecondaryObject", DataType.Bool, 0x16),
            //NStruct("objectHardpoint", "cHardpointInfo", 0x28), // TODO
            //NStruct("secondaryObjectHardpoint", "cHardpointInfo", 0x2c),
            Field("screenShakeScaleLocalPlayer", DataType.Float, 0x30),
            Field("screenShakeScaleEveryone", DataType.Float, 0x34),
            Field("screenShakeScaleLocalPlayerCritical", DataType.Float, 0x38),
            Field("screenShakeScaleEveryoneCritical", DataType.Float, 0x3c)
        );
    }
}
