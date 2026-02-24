using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Vehicles.Equipment;

[Serializable, NetSerializable]
public enum VehicleEquipmentUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class VehicleEquipmentUiState : BoundUserInterfaceState
{
    public readonly NetEntity? Vehicle;
    public VehicleEquipmentUiState(NetEntity? vehicle) => Vehicle = vehicle;
}