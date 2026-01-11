using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
namespace Content.Shared.Starlight.Medical.Surgery.Steps.Parts;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSurgerySystem))] public sealed partial class EyeImplantComponent : Component;
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSurgerySystem))] public sealed partial class HandImplantComponent : Component;
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSurgerySystem))] public sealed partial class BrainImplantComponent : Component;
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSurgerySystem))] public sealed partial class OrganHeartComponent : Component;
[RegisterComponent, NetworkedComponent]
public sealed partial class OrganTongueComponent : Component
{
    [DataField]
    public bool IsMuted;
}

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSurgerySystem))]
public sealed partial class FunctionalOrganComponent : Component
{
    [DataField("comps")]
    public ComponentRegistry? Components;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class OrganDamageComponent : Component
{
    [DataField]
    public DamageSpecifier? Damage;

    [ViewVariables] public DamageSpecifier? StoredDamage;
}
