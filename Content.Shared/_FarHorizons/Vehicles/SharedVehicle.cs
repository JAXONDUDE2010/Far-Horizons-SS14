using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Vehicles;

[Serializable, NetSerializable]
public enum VehicleVisualLayers : byte
{
    AutoAnimate,
}

[Serializable, NetSerializable]
public enum VehicleVisuals : byte
{
    /// <summary>
    /// What layer the vehicle should draw on (assumed integer)
    /// </summary>
    DrawDepth,
    /// <summary>
    /// Whether the wheels should be turning
    /// </summary>
    AutoAnimate
}