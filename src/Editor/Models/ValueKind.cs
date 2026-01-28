// =============================================================================
// DEPRECATED - Moved to Core library as AssetNodeKind
// =============================================================================
// 
// Use ReCap.Parser.AssetNodeKind from the Core library instead.
// This file can be safely deleted.
// =============================================================================

namespace ReCap.Parser.Editor.Models
{
    [Obsolete("Use ReCap.Parser.AssetNodeKind from Core library")]
    public enum ValueKind
    {
        String = ReCap.Parser.AssetNodeKind.String,
        Number = ReCap.Parser.AssetNodeKind.Number,
        Bool = ReCap.Parser.AssetNodeKind.Bool,
        Struct = ReCap.Parser.AssetNodeKind.Struct,
        Array = ReCap.Parser.AssetNodeKind.Array
    }
}
