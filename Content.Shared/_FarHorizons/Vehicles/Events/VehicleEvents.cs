using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Vehicles;

[Serializable, NetSerializable]
public sealed partial class VehicleUnbuckleDoAfter : SimpleDoAfterEvent;