namespace ReCap.Parser.Catalog;

public sealed class sporelabsObject : AssetCatalog
{
    protected override void Build()
    {
        Struct("sporelabsObject", 0x308,
            Field("mTeam", DataType.UInt8, 0x54),
            Field("mbPlayerControlled", DataType.Bool, 0x5c),
            Field("mInputSyncStamp", DataType.UInt32, 0x58),
            Field("mPlayerIdx", DataType.UInt8, 0x55),
            Field("mLinearVelocity", DataType.Vector3, 0x34),
            Field("mAngularVelocity", DataType.Vector3, 0x40),
            Field("mPosition", DataType.Vector3, 0x18),
            Field("mOrientation", DataType.Vector4, 0x24),
            Field("mScale", DataType.Float, 0x10),
            Field("mMarkerScale", DataType.Float, 0x14),
            Field("mLastAnimationState", DataType.UInt32, 0xac),
            Field("mLastAnimationPlayTimeMs", DataType.UInt64, 0xb8),
            Field("mOverrideMoveIdleAnimationState", DataType.UInt32, 0xc0),
            Field("mGraphicsState", DataType.UInt32, 0x258),
            Field("mGraphicsStateStartTimeMs", DataType.UInt64, 0x260),
            Field("mNewGraphicsStateStartTimeMs", DataType.UInt64, 0x268),
            Field("mVisible", DataType.Bool, 0x5f),
            Field("mbHasCollision", DataType.Bool, 0x60),
            Field("mOwnerID", DataType.ObjId, 0x50),
            Field("mMovementType", DataType.UInt8, 0x61),
            Field("mDisableRepulsion", DataType.Bool, 0x284),
            Field("mInteractableState", DataType.UInt32, 0x288),
            Field("sourceMarkerKey.markerId", DataType.UInt32, 0x88)
        );
    }   
}
