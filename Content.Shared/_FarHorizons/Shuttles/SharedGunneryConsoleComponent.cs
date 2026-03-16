using System.Numerics;
using Content.Shared.Shuttles.BUIStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Shuttles;

[Serializable, NetSerializable]
public sealed class GunneryConsoleBuiState : BoundUserInterfaceState
{
    public List<NetEntity> TurretEntities = [];
    public NavInterfaceState State = new(0f, null, null, []);
}

[Serializable, NetSerializable]
public sealed class GunneryConsoleTargetActionMessage(Vector2? position) : BoundUserInterfaceMessage
{
    public Vector2? Position { get; } = position;
}

[Serializable, NetSerializable]
public sealed class GunneryConsoleFireActionMessage(NetCoordinates position, List<NetEntity> turretEntities) : BoundUserInterfaceMessage
{
    public NetCoordinates Position { get; } = position;
    public List<NetEntity> TurretEntities { get; } = turretEntities;
}