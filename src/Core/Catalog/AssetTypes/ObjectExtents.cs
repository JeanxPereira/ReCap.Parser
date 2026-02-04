namespace ReCap.Parser.Catalog;

public sealed class ObjectExtents : AssetCatalog
{
    protected override void Build()
    {
        Struct("ObjectExtents", 0x110,
            IStruct("critterVertical", "ExtentsCategory", 0x0),
            IStruct("critterHorizontal", "ExtentsCategory", 0x10),
            IStruct("minionVertical", "ExtentsCategory", 0x20),
            IStruct("minionHorizontal", "ExtentsCategory", 0x30),
            IStruct("eliteMinionVertical", "ExtentsCategory", 0x40),
            IStruct("eliteMinionHorizontal", "ExtentsCategory", 0x50),
            IStruct("playerTempest", "ExtentsCategory", 0x60),
            IStruct("playerRavager", "ExtentsCategory", 0x70),
            IStruct("playerSentinel", "ExtentsCategory", 0x80),
            IStruct("lieutenantVertical", "ExtentsCategory", 0x90),
            IStruct("lieutenantHorizontal", "ExtentsCategory", 0xa0),
            IStruct("eliteLieutenantVertical", "ExtentsCategory", 0xb0),
            IStruct("eliteLieutenantHorizontal", "ExtentsCategory", 0xc0),
            IStruct("captainVertical", "ExtentsCategory", 0xd0),
            IStruct("captainHorizontal", "ExtentsCategory", 0xe0),
            IStruct("bossVertical", "ExtentsCategory", 0xf0),
            IStruct("bossHorizontal", "ExtentsCategory", 0x100)
        );
    }
}