using Content.Shared._FarHorizons.Medical.SurgeryOverhaul.Components;
using Content.Shared.Starlight.Medical.Surgery.Events;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Robust.Shared.Network;
using Content.Shared.Preferences;
using Robust.Shared.Log;

namespace Content.Shared._FarHorizons.Medical.SurgeryOverhaul.System;

public sealed class SurgeryOverhaulSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly SharedIdentitySystem _identity = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SurgeryAlterAppearanceComponent, SurgeryStepEvent>(OnAlterAppearanceComplete);
    }

    private void OnAlterAppearanceComplete(EntityUid uid, SurgeryAlterAppearanceComponent comp, ref SurgeryStepEvent args)
    {
        var target = args.Body;

        if (!TryComp<HumanoidAppearanceComponent>(target, out var humanoid))
            return;

        if (_net.IsClient)
            return;

        var newProfile = HumanoidCharacterProfile.RandomWithSpecies(humanoid.Species);
        _humanoidAppearance.LoadProfile(target, newProfile, humanoid);
        _metaData.SetEntityName(target, newProfile.Name, raiseEvents: false); 
        _identity.QueueIdentityUpdate(target);
    }
}