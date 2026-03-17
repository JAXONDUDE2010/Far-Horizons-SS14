using Content.Server.Humanoid.Components;
using Content.Shared.Body;
using Content.Shared.Humanoid;
// FarHorizons Start
using Content.Shared.Preferences;
using Robust.Shared.Prototypes; 
using System.Linq; 
using Content.Shared.Cloning;
// FarHorizons End

namespace Content.Server.Humanoid.Systems;

public sealed class RandomHumanoidAppearanceSystem : EntitySystem
{
    [Dependency] private readonly HumanoidProfileSystem _humanoidProfile = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedVisualBodySystem _visualBody = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!; // FarHorizons
    [Dependency] private readonly SharedCloningSystem _cloningSystem = default!; // FarHorizons

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomHumanoidAppearanceComponent, MapInitEvent>(OnMapInit, after: [typeof(SharedVisualBodySystem)]); //FarHorizons
        SubscribeLocalEvent<RandomSpeciesAppearanceComponent, MapInitEvent>(OnMapInit, after: [typeof(SharedVisualBodySystem)]); //FarHorizons
    }

    private void OnMapInit(EntityUid uid, RandomHumanoidAppearanceComponent component, MapInitEvent args)
    {
        // If we have an initial profile/base layer set, do not randomize this humanoid.
        if (!TryComp<HumanoidProfileComponent>(uid, out var humanoid))
            return;

        var profile = HumanoidCharacterProfile.RandomWithSpecies(humanoid.Species);

        _visualBody.ApplyProfileTo(uid, profile);
        _humanoidProfile.ApplyProfileTo(uid, profile);
        _visualBody.MatchMarkingsToSkinColorAndRandomHair(uid, profile); //FarHorizons

        if (component.RandomizeName)
            _metaData.SetEntityName(uid, profile.Name);
    }

    // FarHorizons Start
    private void OnMapInit(EntityUid uid, RandomSpeciesAppearanceComponent component, MapInitEvent args)
    {
        // If we have an initial profile/base layer set, do not randomize this humanoid.
        if (!HasComp<HumanoidProfileComponent>(uid))
            return;

        var profile = HumanoidCharacterProfile.Random();
        var speciesProto = _prototypeManager.Index(profile.Species);
        
        var dummy = Spawn(speciesProto.Prototype);
        if (!_prototypeManager.Resolve(component.TransformCloningSettings, out var settings))
            return;

        _visualBody.CopyAppearanceFrom(dummy, uid);
        _visualBody.ApplyProfileTo(dummy, profile);
        _humanoidProfile.ApplyProfileTo(dummy, profile);

        _cloningSystem.CloneComponents(dummy, uid, settings);
        Del(dummy);
        
        _visualBody.ApplyProfileTo(uid, profile);
        _humanoidProfile.ApplyProfileTo(uid, profile);
        _visualBody.MatchMarkingsToSkinColorAndRandomHair(uid, profile);

        var name = profile.Name; 
        if(component.lastNameOnly)
        {
            name = name.Split(" ").Last();
        }
        if (component.RandomizeName)
            _metaData.SetEntityName(uid, $"{component.namePrefix} {name}");
    }
    // FarHorizons End
}
