using Content.Shared.Body;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Medical;

[Serializable, NetSerializable]
public sealed partial class HealingDoAfterEvent : SimpleDoAfterEvent
{
    public ProtoId<OrganCategoryPrototype>? TargettedLimb; // Far Horizons

    public HealingDoAfterEvent(ProtoId<OrganCategoryPrototype>? limb = null) => TargettedLimb = limb; // Far Horizons
}
