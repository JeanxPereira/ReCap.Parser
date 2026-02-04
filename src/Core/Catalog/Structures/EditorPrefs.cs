namespace ReCap.Parser.Catalog;

public sealed class EditorPrefs : AssetCatalog
{
    protected override void Build()
    {
        Struct("EditorPrefs", 0x9c,
            Field("cameraMode", DataType.Enum, 0x0),
            Field("lastLevel", DataType.Asset, 0x4),
            Field("lastLevelLoadedSuccessfully", DataType.Bool, 0x8),
            Field("lastActiveMarkersetString", DataType.CharPtr, 0x18),
            IStruct("assetBrowser", "cToolPos", 0x20),
            IStruct("propBrowser", "cToolPos", 0x34),
            IStruct("aiEditor", "cToolPos", 0x48),
            IStruct("layers", "cToolPos", 0x5c),
            Field("showEditorWidgets", DataType.Bool, 0x71),
            Field("toolbarVisible", DataType.Bool, 0x70),
            Field("rotateTool", DataType.Bool, 0x9),
            Field("HashFunctionn", DataType.Bool, 0x10),
            Field("scaleTool", DataType.Bool, 0xb),
            Field("transformSpace", DataType.Enum, 0x1c),
            Field("useClipboardCopy", DataType.Bool, 0xc),
            Field("checkoutProtection", DataType.Enum, 0x10),
            Field("newLayerInNewFile", DataType.Bool, 0x14),
            Field("randomRotate", DataType.Bool, 0x72),
            Field("randomScale", DataType.Bool, 0x73),
            Field("randomScalePercent", DataType.Float, 0x74),
            Field("gridUnit", DataType.Float, 0x78),
            Field("snapAngle", DataType.Float, 0x7c),
            ArrayStruct("recentQueries", "cAssetQueryString", 0x80),
            ArrayStruct("recentLayerQueries", "cAssetQueryString", 0x88),
            ArrayStruct("layerPrefs", "cLayerPrefs", 0x90),
            Field("useCurrentPositionAsPlayerStartingPoint", DataType.Bool, 0x98)
        );
    }
}