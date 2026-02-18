using Content.Shared.Atmos;
using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.Vehicles.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleCabinAirComponent : Component
{
    /// <summary>
    /// Target pressure for the mech cabin (kPa).
    /// </summary>
    [DataField, AutoNetworkedField]
    public float TargetPressure = Atmospherics.OneAtmosphere; // ~101.3 kPa

    /// <summary>
    /// Pressure used when metering a single breath (kPa), like a tank regulator.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float RegulatorPressure = 16f;

    /// <summary>
    /// Internal cabin air mixture separate from any attached gas cylinder.
    /// </summary>
    [DataField, AutoNetworkedField]
    public GasMixture Air { get; set; } = new(50f);

    [DataField, AutoNetworkedField]
    public float CooldownRemaining;

    /// <summary>
    /// Total cooldown duration applied after a purge, in seconds.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CooldownDuration = 3f;
}