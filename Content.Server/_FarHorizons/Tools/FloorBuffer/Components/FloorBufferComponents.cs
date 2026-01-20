using Content.Server._FarHorizons.Tools.FloorBuffer.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Tools.FloorBuffer.Components;

[RegisterComponent]
[Access(typeof(FloorBufferSystem))]
public sealed partial class FloorBufferComponent : Component
{
    /// <summary>
    /// Is component enabled?
    /// </summary>
    [DataField]
    public bool Enabled = false;

    /// <summary>
    /// How fast should the user be when the floor buffer is enabled?
    /// </summary>
    [DataField]
    public float SpeedReduction = 0.33f;

    /// <summary>
    /// Solution container name
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string SolutionContainer = "default";

    [DataField]
    public EntProtoId ToggleAction = "ActionToggleZamboni";

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;
}