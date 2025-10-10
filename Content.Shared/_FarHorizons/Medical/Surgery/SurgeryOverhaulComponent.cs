using Robust.Shared.GameStates;
using Content.Shared.Damage;

namespace Content.Shared._FarHorizons.Medical.SurgeryOverhaul.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryBedSpeedComponent : Component
{
    [DataField]
    public float BedSpeedModifier = 2.0f;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryChemicalSpeedComponent : Component
{
    [DataField]
    public float ChemicalSpeedModifier = 1.0f;
}

[RegisterComponent, NetworkedComponent] 
public sealed partial class OnFailDamageComponent : Component
{
    [DataField]
    public DamageSpecifier? Damage;
}
