using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Research.Components;

[RegisterComponent]
public sealed partial class FHResearchTreeComponent : Component
{

    [DataField(required: true)]
    public ProtoId<ResearchTreePrototype> Tree;

    [DataField]
    public HashSet<ProtoId<ResearchTreeNodePrototype>> Researched = [];

    [DataField]
    public Dictionary<ProtoId<ResearchTreeNodePrototype>, int> Progress = [];

    [DataField]
    public List<ProtoId<ResearchTreeNodePrototype>> Queue = [];

    [DataField]
    public int BankedPoints = 0;

    [DataField]
    public int MaxQueueSize = 3;
}

