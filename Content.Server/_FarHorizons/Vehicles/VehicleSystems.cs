using Content.Shared._FarHorizons.Vehicles.EntitySystems;
using Content.Shared._FarHorizons.VehicleBuckle.Components;
using Content.Shared._FarHorizons.Vehicles.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Access.Components;
using System.Numerics;

namespace Content.Server._FarHorizons.Vehicle;

public sealed class VehicleSystems : SharedVehicleSystems
{    
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<VehicleBuckleComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<VehicleBuckleComponent, UnstrappedEvent>(OnUnstrapped);
        SubscribeLocalEvent<VehicleBuckleComponent, UnstrapAttemptEvent>(OnUnstrapAttempt);
        SubscribeLocalEvent<VehicleBuckleComponent, GetAdditionalAccessEvent>(OnGetAdditionalAccess);
        SubscribeLocalEvent<RiderComponent, PullAttemptEvent>(OnPullAttempt);
        _transform.OnGlobalMoveEvent += OnMoveEvent;
    }

    private void OnStrapped(Entity<VehicleBuckleComponent> ent, ref StrappedEvent args)
    {
        if(!TryComp<VehicleComponent>(ent, out var vehicleComp)) return;
        if(vehicleComp.Rider == null)
        {
            EnsureComp<RiderComponent>(args.Buckle.Owner);
            Comp<RiderComponent>(args.Buckle.Owner).Riding = ent.Owner;

            _mover.SetRelay(args.Buckle.Owner, ent.Owner);
            vehicleComp.Rider = args.Buckle.Owner;

            if(TryComp<InputMoverComponent>(ent.Owner, out var mover))
            {
                mover.CanMove = true;
                Dirty(ent.Owner, mover);
            }
        }
    }
    
    private void OnUnstrapped(Entity<VehicleBuckleComponent> ent, ref UnstrappedEvent args)
    {
        if(!TryComp<VehicleComponent>(ent, out var vehicleComp)) return;

        if(HasComp<RelayInputMoverComponent>(args.Buckle.Owner))
            RemComp<RelayInputMoverComponent>(args.Buckle.Owner);
        if(HasComp<RiderComponent>(args.Buckle.Owner))
            RemComp<RiderComponent>(args.Buckle.Owner);
        vehicleComp.Rider = null;
    }

    private void OnUnstrapAttempt(Entity<VehicleBuckleComponent> ent, ref UnstrapAttemptEvent args)
    {
        if(!TryComp<VehicleComponent>(ent, out var vehicleComp)) return;
        if (vehicleComp.Rider != args.User)
            args.Cancelled = true;
    }

    private void OnGetAdditionalAccess(Entity<VehicleBuckleComponent> ent, ref GetAdditionalAccessEvent args)
    {
        if(!TryComp<VehicleComponent>(ent, out var vehicleComp)) return;
        if (vehicleComp.Rider == null) return;

        args.Entities.Add(vehicleComp.Rider.Value);
    }

    private void OnPullAttempt(Entity<RiderComponent> ent, ref PullAttemptEvent args)
    {
        if (ent.Owner != args.PullerUid)
            args.Cancelled = true;
    }
    
    private void OnMoveEvent(ref MoveEvent ev)
    {
        var rider = ev.Entity.Owner;
        if(!TryComp<RiderComponent>(rider, out var ridercomp)) return;
        var riderTransform = Transform(rider);
        if(riderTransform.ParentUid !=  ridercomp.Riding) return;
        
        if(riderTransform.LocalPosition.X != 0 || riderTransform.LocalPosition.Y != 0)
            _transform.SetLocalPosition(rider, new Vector2(0f, 0f), riderTransform);
        if(riderTransform.LocalRotation != 0)
            _transform.SetLocalRotation(rider, 0f, riderTransform);
    }
}