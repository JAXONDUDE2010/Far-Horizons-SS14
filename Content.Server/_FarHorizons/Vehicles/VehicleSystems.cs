using Content.Shared._FarHorizons.Vehicles.EntitySystems;
using Content.Shared._FarHorizons.Vehicles.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Pulling.Events;

namespace Content.Server._FarHorizons.Vehicle;

public sealed class VehicleSystems : SharedVehicleSystems
{    
    [Dependency] private readonly SharedMoverController _mover = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<VehicleComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<VehicleComponent, UnstrappedEvent>(OnUnstrapped);
        SubscribeLocalEvent<VehicleComponent, UnbuckleAttemptEvent>(OnUnbuckleAttempt);
        SubscribeLocalEvent<RiderComponent, PullAttemptEvent>(OnPullAttempt);
    }

    private void OnStrapped(Entity<VehicleComponent> ent, ref StrappedEvent args)
    {
        if(ent.Comp.Rider == null)
        {
            EnsureComp<RiderComponent>(args.Buckle.Owner);
            Comp<RiderComponent>(args.Buckle.Owner).Riding = ent.Owner;

            _mover.SetRelay(args.Buckle.Owner, ent.Owner);
            ent.Comp.Rider = args.Buckle.Owner;

            if(TryComp<InputMoverComponent>(ent.Owner, out var mover))
            {
                mover.CanMove = true;
                Dirty(ent.Owner, mover);
            }
        }
    }
    
    private void OnUnstrapped(Entity<VehicleComponent> ent, ref UnstrappedEvent args)
    {
        if(HasComp<RelayInputMoverComponent>(args.Buckle.Owner))
            RemComp<RelayInputMoverComponent>(args.Buckle.Owner);
        if(HasComp<RiderComponent>(args.Buckle.Owner))
            RemComp<RiderComponent>(args.Buckle.Owner);
        ent.Comp.Rider = null;
    }

    private void OnUnbuckleAttempt(Entity<VehicleComponent> ent, ref UnbuckleAttemptEvent args)
    {
        if (ent.Comp.Rider != args.User)
            args.Cancelled = true;
    }

    private void OnPullAttempt(Entity<RiderComponent> ent, ref PullAttemptEvent args)
    {
        if (ent.Owner != args.PullerUid)
            args.Cancelled = true;
    }
}