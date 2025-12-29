using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.VehicleContainer.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VehicleContainerComponent : Component
{
    /// <summary>
    /// check if a vehicle requires ignition before allowing it to move
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Started = false;
}