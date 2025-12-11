using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Research;

[Prototype]
public sealed partial class ResearchTreeUnlockFlagPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
    [DataField(required: true)]
    public LocId Text = default!;
}