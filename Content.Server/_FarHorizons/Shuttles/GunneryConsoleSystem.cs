using System.Linq;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Shuttles.Systems;
using Content.Shared._FarHorizons.Shuttles;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._FarHorizons.Shuttles;

public sealed class GunneryConsoleSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _signal = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = null!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private readonly ShuttleConsoleSystem _console = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunneryConsoleComponent, MapInitEvent>(OnMapInit);
        
        SubscribeLocalEvent<GunneryConsoleComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<GunneryConsoleComponent, PortDisconnectedEvent>(OnPortDisconnected);
        
        SubscribeLocalEvent<GunneryConsoleComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<GunneryConsoleComponent, GunneryConsoleFireActionMessage>(OnFireAction);
    }

    private void OnMapInit(EntityUid uid, GunneryConsoleComponent comp, ref MapInitEvent args)
    {
        _signal.EnsureSourcePorts(uid, comp.TurretConnectionPort);

        CollectTurrets(uid, comp);
    }

    private void CollectTurrets(EntityUid uid, GunneryConsoleComponent comp)
    {
        if (!EntityManager.TryGetComponent<DeviceLinkSourceComponent>(uid, out var source))
            return;

        comp.ConnectedTurrets.Clear();

        foreach (var (turretUid, _) in source.LinkedPorts)
        {
            if (!EntityManager.HasComponent<GunComponent>(turretUid))
                continue;

            comp.ConnectedTurrets.Add(turretUid);
        }
    }

    private void OnNewLink(EntityUid uid, GunneryConsoleComponent comp, ref NewLinkEvent args)
    {
        if (args.SourcePort != comp.TurretConnectionPort)
            return;

        if (!EntityManager.HasComponent<GunComponent>(args.Sink))
            return;

        if(comp.ConnectedTurrets.Contains(args.Sink))
            return;

        comp.ConnectedTurrets.Add(args.Sink);
        UpdateUI(uid, comp);
    }

    private void OnPortDisconnected(EntityUid uid, GunneryConsoleComponent comp, ref PortDisconnectedEvent args)
    {
        if (args.Port != comp.TurretConnectionPort)
            return;

        comp.ConnectedTurrets.Remove(args.Sink);
        UpdateUI(uid, comp);
    }

    private void OnUIOpened(EntityUid uid, GunneryConsoleComponent comp, ref BoundUIOpenedEvent args) => UpdateUI(uid, comp);

    private void UpdateUI(EntityUid uid, GunneryConsoleComponent comp)
    {
        if (!_uiSystem.IsUiOpen(uid, RadarConsoleUiKey.Key))
            return;

        GunneryConsoleBuiState state = new();
        comp.ConnectedTurrets.ForEach(turret => state.TurretEntities.Add(GetNetEntity(turret)));
        state.State = GetNavState(uid);
        _uiSystem.SetUiState(uid, RadarConsoleUiKey.Key, state);
    }

    private NavInterfaceState GetNavState(EntityUid uid)
    {
        var xform = Transform(uid);
        var onGrid = xform.ParentUid == xform.GridUid;
        EntityCoordinates? coordinates = onGrid ? xform.Coordinates : null;
        Angle? angle = onGrid ? xform.LocalRotation : null;

        return coordinates != null && angle != null
            ? _console.GetNavState(uid, _console.GetAllDocks(), coordinates.Value, angle.Value)
            : _console.GetNavState(uid, _console.GetAllDocks());
    }

    private void OnFireAction(EntityUid uid, GunneryConsoleComponent comp, ref GunneryConsoleFireActionMessage args)
    {
        List<EntityUid> turretUids = [];
        args.TurretEntities.ForEach(netEnt => turretUids.Add(EntityManager.GetEntity(netEnt)));
        turretUids = [.. turretUids.Intersect(comp.ConnectedTurrets)];

        var targetCoords = EntityManager.GetCoordinates(args.Position);
        
        foreach (var turret in turretUids)
        {
            if (!EntityManager.TryGetComponent<GunComponent>(turret, out var gun))
                continue;

            if(!_gunSystem.CanShoot(gun))
                continue;
            
            
            Timer.Spawn(_random.Next(0, 300), () =>
                _gunSystem.AttemptShoot(turret, (turret, gun), targetCoords)
            );
        }
    }
}