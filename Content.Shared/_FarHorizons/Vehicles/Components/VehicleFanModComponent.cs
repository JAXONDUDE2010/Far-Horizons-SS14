using Content.Shared.Atmos;
using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.Vehicles.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleFanModComponent : Component
{
    /// <summary>
    /// Whether the fan is currently active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsActive;

    /// <summary>
    /// Current fan state see <see cref="MechFanState"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FanState State = FanState.Off;

    /// <summary>
    /// How much gas the fan can process per second when active.
    /// </summary>
    [DataField]
    public float GasProcessingRate = 1f;

    /// <summary>
    /// Whether the attached filter should be active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool FilterEnabled = true;

    /// <summary>
    /// Gases that will be filtered during fan operation.
    /// </summary>
    [DataField(required: true)]
    public HashSet<Gas> FilterGases = new();
}

public enum FanState
{
    Off,
    On,
    Idle
}