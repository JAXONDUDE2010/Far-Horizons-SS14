using System.Linq;
using Content.Shared._FarHorizons.LimbDamage.Components;
using Content.Shared.Body;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.LimbDamage.Effect;

[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class LimbEffectDetach : ILimbDamageEffect
{
    private static readonly SoundSpecifier? _gibSound = new SoundCollectionSpecifier("gib", AudioParams.Default.WithVariation(0.025f));

    public void Run(Entity<DamageableLimbComponent> limb, IEntityManager entMan, IPrototypeManager protoMan)
    {
        if (limb.Comp.Organ?.Body == null)
            return;

        var body = limb.Comp.Organ.Body.Value;

        entMan.System<SharedContainerSystem>().TryRemoveFromContainer(limb.Owner);
        entMan.System<SharedAudioSystem>().PlayPredicted(_gibSound, body, null);
    }
        
}

[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class LimbEffectDestroy : ILimbDamageEffect
{
    private static readonly SoundSpecifier? _gibSound = new SoundCollectionSpecifier("gib", AudioParams.Default.WithVariation(0.025f));

    public void Run(Entity<DamageableLimbComponent> limb, IEntityManager entMan, IPrototypeManager protoMan)
    {
        if (limb.Comp.Organ?.Body == null ||
            !entMan.TryGetComponent<BodyComponent>(limb.Comp.Organ.Body.Value, out var body))
            return;

        var connectedOrgans = body.Organs?.ContainedEntities.Where(p =>
            entMan.TryGetComponent<OrganComponent>(p, out var organ) &&
            protoMan.Index(organ.Category)?.ConnectsTo == limb.Comp.Organ.Category).ToList();
        
        if (connectedOrgans == null)
            return;

        var container = entMan.System<SharedContainerSystem>();

        foreach (var organ in connectedOrgans)
            container.TryRemoveFromContainer(organ);

        entMan.System<SharedAudioSystem>().PlayPredicted(_gibSound, limb.Comp.Organ.Body.Value, null);
        entMan.QueueDeleteEntity(limb);
    }
        
}