namespace ReCap.Parser.Catalog;

public sealed class Objective : AssetCatalog
{
    protected override void Build()
    {
        Struct("objective", 0x84,
            Field("namespace", DataType.Char, 0x14, 0x50),
            Field("descriptionText", DataType.UInt32, 0x64),
            Field("progressText", DataType.UInt32, 0x68),
            Field("errorText", DataType.UInt32, 0x6c),
            Field("groupObjective", DataType.Bool, 0x70),
            Field("requiredObjective", DataType.Bool, 0x78),
            Field("timeObjectiveRequires", DataType.Bool, 0x79),
            Field("handledEvents", DataType.UInt32, 0x74),
            Field("progressVoiceOver", DataType.UInt32, 0x7c),
            Field("completedVoiceOver", DataType.UInt32, 0x80)
        );
    }   
}
