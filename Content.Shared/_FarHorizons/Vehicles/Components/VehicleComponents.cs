using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.Vehicles.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class VehicleComponent : Component
{
    /// <summary>
    /// The person in control of this vehicle
    /// </summary>
    [DataField("rider")]
    public EntityUid? Rider;
    
    [DataField]
    public string? BaseState;

    [DataField("autoAnimate")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool AutoAnimate = true;
}