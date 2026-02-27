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
    public readonly int Power;
    public readonly int Integrity;
    public VehicleEquipmentUiState(NetEntity? vehicle, int power, int integrity)
    {
        Vehicle = vehicle;
        Power = power;
        Integrity = integrity;
    }
}
