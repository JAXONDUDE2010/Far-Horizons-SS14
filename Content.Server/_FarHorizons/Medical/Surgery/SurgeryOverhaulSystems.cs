using Content.Shared._FarHorizons.Medical.SurgeryOverhaul.Components;
using Content.Shared.Starlight.Medical.Surgery.Events;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Preferences;
using Content.Shared.Damage;
using Content.Shared.Research.Components;
using Content.Shared.Research.Systems;
using Content.Shared.Research.Prototypes;
using Content.Shared.Buckle.Components;
using Content.Shared.DeviceLinking;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Content.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.Starlight.Medical.Surgery;
using Content.Shared.Starlight.Medical.Surgery.Steps.Parts;
using Content.Shared.Body.Part;

namespace Content.Server._FarHorizons.Medical.SurgeryOverhaul.Systems;

public sealed partial class SurgeryOverhaulSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly SharedIdentitySystem _identity = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly SharedResearchSystem _research = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    private readonly List<EntProtoId> _surgeriesForRotten = [];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SurgeryAlterAppearanceComponent, SurgeryStepCompleteEvent>(OnAlterAppearanceComplete);
        SubscribeLocalEvent<HealDamageComponent, SurgeryStepCompleteEvent>(OnHealDamageComplete);
        SubscribeLocalEvent<NecrosisSurgeryComponent, SurgeryStepCompleteEvent>(OnNecrosisComplete);

        LoadSurgeriesForRotten();
    }

    private void OnAlterAppearanceComplete(EntityUid uid, SurgeryAlterAppearanceComponent comp, ref SurgeryStepCompleteEvent args)
    {
        if (_net.IsClient) return;
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

    private void OnHealDamageComplete(EntityUid uid, HealDamageComponent comp, ref SurgeryStepCompleteEvent args)
    {
        if (_net.IsClient) return;
        var StepProto = _prototypes.Index<EntityPrototype>(args.StepProto);
        var ResearchModifier = 75f;
        TechnologyDatabaseComponent? TechDatabase = new();
        DamageSpecifier BonusHeal = new();
        DamageSpecifier TotalHeal;
        if (StepProto.TryGetComponent<HealDamageComponent>(out var healComp))
        {
            if (TryComp(args.Body, out BuckleComponent? buckle) && TryComp(buckle.BuckledTo, out DeviceLinkSinkComponent? linkComp))
            {
                foreach (var source in linkComp.LinkedSources)
                {
                    if (TryComp(source, out TechnologyDatabaseComponent? techComp))
                    {
                        TechDatabase = techComp;
                        break;
                    }
                }
            }
            foreach (var (key, value) in healComp.TechnologyModifier!)
            {
                var TechProto = _prototypes.Index<TechnologyPrototype>(key.Id);
                if (_research.IsTechnologyUnlocked(uid, TechProto, TechDatabase) && ResearchModifier > value)
                    ResearchModifier = value;
            }
            if (TryComp<DamageableComponent>(args.Body, out var dmgComp))
            {
                foreach (var key in healComp.Heal!.DamageDict.Keys)
                {
                    BonusHeal.DamageDict.Add(key, dmgComp.TotalDamage / ResearchModifier);
                }
            }
            TotalHeal = healComp.Heal! + (-BonusHeal);
            _damageableSystem.TryChangeDamage(args.Body, TotalHeal);
        }
    }
    private void OnNecrosisComplete(EntityUid uid, NecrosisSurgeryComponent comp, ref SurgeryStepCompleteEvent args)
    {
        if (_net.IsClient) return;
        LoadSurgeriesForRotten();

        var surgProto = _prototypes.Index<EntityPrototype>(args.SurgeryProto);

        if (!surgProto.TryGetComponent<NecrosisSurgeryComponent>(out var surgComp))
            return;

        Logger.Info($"Surg Comp: {surgComp}");
        surgComp.RequiredSurgeries.Clear();

        for (int i = 0; i < surgComp.AmountofSurgeries; i++)
        {
            EntProtoId? chosenSurgery = null;

            while (true)
            {
                if (_surgeriesForRotten.Count == 0)
                    break;
                chosenSurgery = _random.PickAndTake(_surgeriesForRotten);
                if (!TryComp<BodyPartComponent>(args.Part, out var partComp))
                    return;
                if(surgProto.TryGetComponent<SurgeryStepOrganExtractComponent>(out var extractComp))
                {
                    continue;
                }
                break;
            }
            surgComp.RequiredSurgeries.Add((EntProtoId)chosenSurgery!);
        }

        foreach (var test in surgComp.RequiredSurgeries)
            Logger.Info($"Test Id: {test.Id}");
    }

    private void LoadSurgeriesForRotten()
    {
        _surgeriesForRotten.Clear();

        foreach (var entity in _prototypes.EnumeratePrototypes<EntityPrototype>())
        {
            var surgProto = _prototypes.Index<EntityPrototype>(entity);
            if (surgProto.HasComponent<NecrosisSurgeryStepComponent>())
                _surgeriesForRotten.Add(new EntProtoId(entity.ID));
        }
    }
}