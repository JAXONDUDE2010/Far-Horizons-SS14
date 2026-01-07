using Content.Shared._FarHorizons.Vehicles.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Verbs;
using Robust.Server.Physics;

namespace Content.Server._FarHorizons.Towing;
public sealed partial class TowingSystem : EntitySystem
{    
    [Dependency] private readonly JointSystem _joint = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<TowingComponent, GetVerbsEvent<UtilityVerb>>(OnAddUtilityVerb);
        base.Initialize();
    }

    public void OnAddUtilityVerb(EntityUid ent, TowingComponent component, GetVerbsEvent<UtilityVerb> args)
    {
        if(!args.CanAccess || !args.CanInteract || args.Hands == null) return;
        
        var tieVerb = new UtilityVerb
        {
            
        };
        args.Verbs.Add(tieVerb);
    }
}