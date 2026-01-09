using Content.Shared._FarHorizons.Towing.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Verbs;
using Robust.Server.Physics;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Pulling.Components;

namespace Content.Server._FarHorizons.Towing;
public sealed partial class TowingSystem : EntitySystem
{    
    [Dependency] private readonly JointSystem _joint = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<TowingComponent, GetVerbsEvent<UtilityVerb>>(OnAddUtilityVerb);
        SubscribeLocalEvent<TiedComponent, GetVerbsEvent<AlternativeVerb>>(OnAddAlternativeVerb);
        SubscribeLocalEvent<PullableComponent, TieUpDoAfter>(OnTieUpDoAfter);
        SubscribeLocalEvent<PullableComponent, UnTieDoAfter>(OnUnTieDoAfter);
        base.Initialize();
    }

    public void OnAddUtilityVerb(EntityUid ent, TowingComponent component, GetVerbsEvent<UtilityVerb> args)
    {
        if(!args.CanAccess || !args.CanInteract || args.Hands == null) return;
        
        var tieVerb = new UtilityVerb
        {
            Text = "Tie Up",
            Act = () =>
            {
                    var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, component.TieUpTime, new TieUpDoAfter(), args.Target, target: args.Target)
                    {
                        BreakOnMove = true,
                    };
                    
                    _doAfter.TryStartDoAfter(doAfterEventArgs);
            }
        };
        args.Verbs.Add(tieVerb);
    }

    public void OnTieUpDoAfter(Entity<PullableComponent> ent, ref TieUpDoAfter args)
    {
        Logger.Info($"{args.Target}");
        if(args.Cancelled) return;
        var target = args.Target!.Value;
        EnsureComp<TiedComponent>(target);
    }

    public void OnAddAlternativeVerb(EntityUid ent, TiedComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if(!args.CanAccess || !args.CanInteract || args.Hands == null) return;
        if(!HasComp<TiedComponent>(args.Target)) return;

        var untieVerb = new AlternativeVerb
        {
            Text = "Untie",
            Act = () =>
            {
                    var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, component.UntieTime, new UnTieDoAfter(), args.Target, target: args.Target)
                    {
                        BreakOnMove = true,
                    };
                    
                    _doAfter.TryStartDoAfter(doAfterEventArgs);
            }
        };
        args.Verbs.Add(untieVerb);
    }

    public void OnUnTieDoAfter(Entity<PullableComponent> ent, ref UnTieDoAfter args)
    {
        if(args.Cancelled) return;

        var target = args.Target!.Value;
        RemComp<TiedComponent>(target);
    }
}