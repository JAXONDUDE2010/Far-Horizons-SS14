using Content.Server.Popups;
using Content.Shared._FarHorizons.Silicons.Glitching;
using Content.Shared._FarHorizons.VFX;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._FarHorizons.Silicons.Glitching;

public sealed class GlitchOnIonStormSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public void TriggerIonStorm(Entity<GlitchOnIonStormComponent> ent)
    {
        var comp = EnsureComp<GlitchingEffectComponent>(ent.Owner);
        comp.Animated = true;
        comp.StartAt = _timing.CurTime;
        comp.FinishAt = _timing.CurTime + ent.Comp.Duration;
        comp.RampDuration = ent.Comp.Ramp;
        Dirty<GlitchingEffectComponent>((ent.Owner, comp));
        _popup.PopupEntity(Loc.GetString("glitch-on-ion-storm-start-message"), ent.Owner, ent.Owner, PopupType.LargeCaution);
    }
}