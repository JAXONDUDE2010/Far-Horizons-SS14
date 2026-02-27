using Content.Shared._FarHorizons.GenericFieldGenerator.Components;
using Content.Shared.Destructible;

namespace Content.Server._FarHorizons.GenericFieldGenerator.EntitySystems;

public sealed class GenericFieldSystem : EntitySystem
{
    [Dependency] private readonly GenericFieldGeneratorSystem _genericgen = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GenericFieldComponent, DestructionEventArgs>(OnDestructionEvent);
            SubscribeLocalEvent<GenericFieldComponent, MapInitEvent>(OnMapInit);
        }

    private void OnDestructionEvent(Entity<GenericFieldComponent> field, ref DestructionEventArgs args)
    {
        if(field.Comp.SourceGen == null)
        return;
        _genericgen.FieldDestroyed(field.Comp.SourceGen.Value);
    }

    private void OnMapInit(Entity<GenericFieldComponent> field, ref MapInitEvent args)
    {
        return;
    }
}