using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Vehicles.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class VehicleEquipmentComponent : Component
{
    [DataField]
    public EntProtoId? ActionProto;
    
    [ViewVariables, AutoNetworkedField] 
    public EntityUid? ActionEntity;

    [DataField(required: true)]
    public EquipmentType Slot = EquipmentType.NONE;
}

[Serializable, NetSerializable]
[Flags]
public enum EquipmentType
{
    NONE = 0,
    TIRES = 1 << 0,
    ENGINE = 1 << 1,
    HEADLIGHT = 1 << 2,
    LIGHTBAR = 1 << 3,
    AIRTANK = 1 << 4,
    VENTFAN = 1 << 5,
    FUELTANK = 1 << 6,
}