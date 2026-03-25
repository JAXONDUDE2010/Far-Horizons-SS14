using Content.Shared.Medical.Disease.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Medical.Disease.Systems;

[Serializable, NetSerializable]
public sealed class DiseaseData
{
    [ViewVariables]
    public ProtoId<DiseasePrototype> Id;

    [ViewVariables]
    public string StrainName = string.Empty;
}