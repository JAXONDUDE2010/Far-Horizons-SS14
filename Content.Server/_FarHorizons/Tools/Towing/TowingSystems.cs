using Content.Shared._FarHorizons.Towing.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Verbs;
using Robust.Server.Physics;
using Content.Shared.DoAfter;
using Robust.Shared.Physics;
using Content.Shared.Physics;
using Robust.Shared.Physics.Components;
using Content.Shared.Coordinates;
using Content.Shared.Hands.Components;
using Content.Shared.Movement.Systems;

namespace Content.Server._FarHorizons.Towing;
public sealed partial class TowingSystem : EntitySystem
{    
    [Dependency] private readonly JointSystem _joint = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    private static readonly string _towingRope = "TowingRope";
    private static readonly string _hitchHook = "HitchHook";
    public override void Initialize()
    {
        SubscribeLocalEvent<TowingRopeComponent, GetVerbsEvent<UtilityVerb>>(OnAddUtilityVerb);
        SubscribeLocalEvent<TiedComponent, GetVerbsEvent<AlternativeVerb>>(OnAddUnTieVerb);
        SubscribeLocalEvent<HitchComponent, GetVerbsEvent<AlternativeVerb>>(OnAddHitchVerb);
        SubscribeLocalEvent<TowingRopeComponent, TieUpDoAfter>(OnTieUpDoAfter);
        SubscribeLocalEvent<TiedComponent, UnTieDoAfter>(OnUnTieDoAfter);
        SubscribeLocalEvent<HandsComponent, DeployHitchDoAfter>(OnDeployHitchDoAfter);
        SubscribeLocalEvent<TiedComponent, JointRemovedEvent>(OnJointRemoved);
        base.Initialize();
    }

    public void OnAddUtilityVerb(EntityUid ent, TowingRopeComponent component, GetVerbsEvent<UtilityVerb> args)
    {
        if(!args.CanAccess || !args.CanInteract || args.Hands == null) return;
        if(HasComp<TiedComponent>(args.Target)) return;
        if(TryComp<PhysicsComponent>(args.Target, out var physicsComponent) && physicsComponent.BodyType == BodyType.Static) return;
        var tieVerb = new UtilityVerb
        {
            Text = Loc.GetString("towing-rope-tie"),
            Act = () =>
            {
                    var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, component.TieUpTime, new TieUpDoAfter(), ent, target: args.Target, used: ent)
                    {
                        BreakOnMove = true,
                    };
                    
                    _doAfter.TryStartDoAfter(doAfterEventArgs);
            }
        };
        args.Verbs.Add(tieVerb);
    }

    public void OnTieUpDoAfter(Entity<TowingRopeComponent> ent, ref TieUpDoAfter args)
    {
        if(args.Cancelled) return;
        if(args.Target == null || args.Used == null) return;
        var target = args.Target.Value;
        var used = args.Used.Value;
        args.Handled = true;
        
        if(ent.Comp.FirstEnd == null || !EntityManager.EntityExists(ent.Comp.FirstEnd))
        {
            CreateJoint(used, target, ent.Comp);
        }
        else
        {
            var entA = ent.Comp.FirstEnd.Value;
            _joint.RecursiveClearJoints(entA);

            CreateJoint(entA, target, ent.Comp);
            QueueDel(ent.Owner);
        }
    }

    public void OnAddUnTieVerb(EntityUid ent, TiedComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if(!args.CanAccess || !args.CanInteract || args.Hands == null) return;
        if(!HasComp<TiedComponent>(args.Target)) return;
        var untieRopeVerb = new AlternativeVerb
        {
            Text = Loc.GetString("towing-rope-untie"),
            Act = () =>
            {
                    var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, component.UntieTime, new UnTieDoAfter(), args.Target, target: args.Target)
                    {
                        BreakOnMove = true,
                        BreakOnDamage = true,
                        BreakOnDropItem = true,
                        BreakOnHandChange = true
                    };
                        
                    _doAfter.TryStartDoAfter(doAfterEventArgs);
            }
        };
        args.Verbs.Add(untieRopeVerb);
    }

    public void OnUnTieDoAfter(Entity<TiedComponent> ent, ref UnTieDoAfter args)
    {
        if(args.Cancelled) return;
        if(args.Target == null) return;
        var target = args.Target.Value;
        if(TryComp<TiedComponent>(target, out var tComp))
        {
            if(tComp.AttachedTo != null)
            {
                var attachedTo = tComp.AttachedTo.Value;
                _movementSpeed.RefreshMovementSpeedModifiers(attachedTo);
                RemComp<TiedComponent>(attachedTo);
                RemComp<JointComponent>(attachedTo);
                RemComp<JointVisualsComponent>(attachedTo);
                if(!tComp.isHitch)
                {
                    var newrope = SpawnAtPosition(_towingRope, target.ToCoordinates());
                    _hands.TryPickupAnyHand(args.User, newrope);   
                }             
            }
        }
        _movementSpeed.RefreshMovementSpeedModifiers(target);    
        RemComp<TiedComponent>(target);
        RemComp<JointComponent>(target);
        RemComp<JointVisualsComponent>(target);
        _joint.RecursiveClearJoints(target);
    }

    public void OnAddHitchVerb(EntityUid uid, HitchComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if(!args.CanAccess || !args.CanInteract || args.Hands == null) return;
        if(HasComp<TiedComponent>(args.Target)) return;
        if(TryComp<TowingRopeComponent>(args.Target, out var TowComp) && TowComp.FirstEnd != null) return;
        if(Transform(args.Target).ParentUid != Transform(args.Target).GridUid) return;
        var untieRopeVerb = new AlternativeVerb
        {
            Text = Loc.GetString("towing-hitch-deploy"),
            Act = () =>
            {
                    var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, component.HitchDeploy, new DeployHitchDoAfter(), args.User, target: args.Target)
                    {
                        BreakOnMove = true,
                        BreakOnDamage = true,
                        BreakOnDropItem = true,
                        BreakOnHandChange = true
                    };
                        
                    _doAfter.TryStartDoAfter(doAfterEventArgs);
            }
        };
        args.Verbs.Add(untieRopeVerb);
    }

    public void OnDeployHitchDoAfter(Entity<HandsComponent> ent, ref DeployHitchDoAfter args)
    {
        if(args.Target == null || args.Cancelled) return;
        var target = args.Target.Value;

        var hook = SpawnAtPosition(_hitchHook, target.ToCoordinates());
        _hands.TryPickupAnyHand(args.User, hook);
        
        var towComp = Comp<TowingRopeComponent>(hook);
        CreateJoint(hook, target, towComp);
    }

    private void CreateJoint(EntityUid entityA, EntityUid entityB, TowingRopeComponent towComp)
    {        
        var visualComp = EnsureComp<JointVisualsComponent>(entityB);
        var tiedComp = EnsureComp<TiedComponent>(entityB);
        var joint = _joint.CreateDistanceJoint(entityA, entityB);
        joint.MaxLength = joint.Length + 0.3f;
        joint.Stiffness = 1f;
        joint.MinLength = 0.0f;
                        
        visualComp.Sprite = towComp.RopeSprite;
        visualComp.Target = entityA;
        tiedComp.AttachedTo = entityA;

        if (TryComp<TiedComponent>(entityA, out var tied))
        {
            tied.AttachedTo = entityB;
        }

        towComp.FirstEnd = entityB;
        
        _movementSpeed.RefreshMovementSpeedModifiers(entityA);
        _movementSpeed.RefreshMovementSpeedModifiers(entityB);
    }

    private void OnJointRemoved(Entity<TiedComponent> ent, ref JointRemovedEvent args)
    {
        if(!HasComp<TiedComponent>(args.OurEntity) || !HasComp<TiedComponent>(args.OtherEntity)) return;
        if(ent.Comp.AttachedTo != null)
        {
            RemComp<JointVisualsComponent>(ent.Comp.AttachedTo.Value);
            RemComp<TiedComponent>(ent.Comp.AttachedTo.Value);
        }
        RemComp<JointVisualsComponent>(ent.Owner);
        RemComp<TiedComponent>(ent.Owner);
    }
}