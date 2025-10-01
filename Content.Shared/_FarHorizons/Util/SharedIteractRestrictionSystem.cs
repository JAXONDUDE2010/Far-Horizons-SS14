using Content.Shared._FarHorizons.Util.Components;
using Content.Shared.Tag;

namespace Content.Shared._FarHorizons.Util;

public sealed class InteractRestrictionSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InteractRestrictionComponent, CheckItemCanBeUsedEvent>(CheckItemCanBeUsed);
    }

    private void CheckItemCanBeUsed(Entity<InteractRestrictionComponent> ent, ref CheckItemCanBeUsedEvent ev)
    {
        if (ev.Cancelled)
            return;

        var target = ev.Target;
        var user = ev.User;

        if (target != null && ent.Comp.RestrictInteractionTarget is InteractRestrictionList targetRestrict) {
            if (targetRestrict.Blacklist != null &&
                _tagSystem.HasAnyTag(target.Value, targetRestrict.Blacklist))
                    ev.Cancel();
            
            if (targetRestrict.Whitelist != null &&
                !_tagSystem.HasAnyTag(target.Value, targetRestrict.Whitelist))
                    ev.Cancel();
        }

        if (ent.Comp.RestrictInteractionSource is InteractRestrictionList sourceRestrict) {
            if (sourceRestrict.Blacklist != null &&
                _tagSystem.HasAnyTag(user, sourceRestrict.Blacklist))
                    ev.Cancel();
            
            if (sourceRestrict.Whitelist != null &&
                !_tagSystem.HasAnyTag(user, sourceRestrict.Whitelist))
                    ev.Cancel();
        }
    }
}