using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.Vehicle.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class VehicleComponent : Component
{
    /// <summary>
    /// The person in control of this vehicle
    /// </summary>
    [DataField("rider")]
    public EntityUid? Rider;
}