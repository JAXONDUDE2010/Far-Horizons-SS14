using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Content.Shared.Actions;

namespace Content.Shared._FarHorizons.Vehicles;

[Serializable, NetSerializable]
public sealed partial class VehicleUnbuckleDoAfter : SimpleDoAfterEvent;

public sealed partial class TurnKeysEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class TurnKeysDoAfter : SimpleDoAfterEvent;