namespace ReCap.Parser.Catalog;

public sealed class ServerEvent : AssetCatalog
{
    protected override void Build()
    {
        Struct("ServerEvent", 0x98,
            Field("simpleSwarmEffectID", DataType.UInt32, 0x4),
            Field("objectFxIndex", DataType.UInt8, 0x8),
            Field("bRemove", DataType.Bool, 0x4),
            Field("bHardStop", DataType.Bool, 0xa),
            Field("bForceAttach", DataType.UInt32, 0xb),
            Field("bCritical", DataType.UInt32, 0xc),
            Field("asset", DataType.Asset, 0x10),
            Field("objectId", DataType.ObjId, 0x18),
            Field("secondaryObjectId", DataType.ObjId, 0x1c),
            Field("attackerId", DataType.ObjId, 0x20),
            Field("position", DataType.Vector3, 0x24),
            Field("facing", DataType.Vector3, 0x30),
            Field("orientation", DataType.Vector4, 0x3c),
            Field("targetPoint", DataType.Vector3, 0x4c),
            Field("textValue", DataType.Int, 0x58),
            Field("clientEventID", DataType.UInt32, 0x5c),
            Field("clientIgnoreFlags", DataType.UInt8, 0x60),
            Field("lootReferenceId", DataType.UInt64, 0x68),
            Field("lootInstanceId", DataType.UInt64, 0x70),
            Field("lootRigblockId", DataType.UInt32, 0x78),
            Field("lootSuffixAssetId", DataType.UInt32, 0x7c),
            Field("lootPrefixAssetId1", DataType.UInt32, 0x80),
            Field("lootPrefixAssetId2", DataType.UInt32, 0x84),
            Field("lootItemLevel", DataType.Int, 0x88),
            Field("lootRarity", DataType.Int, 0x8c),
            Field("lootCreationTime", DataType.UInt32, 0x90)
        );
    }
}
