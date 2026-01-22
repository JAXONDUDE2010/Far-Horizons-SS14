using Content.Shared._FarHorizons.Tools.FloorBuffer.Components;
using Content.Shared.Toggleable;

namespace Content.Shared._FarHorizons.Tools.FloorBuffer.Systems;
public abstract partial class SharedFloorBufferSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<FloorBufferComponent, ToggleActionEvent>(OnToggleAction);

        base.Initialize();
    }

    protected virtual void OnToggleAction(EntityUid uid, FloorBufferComponent component, ToggleActionEvent args)
    {
        if (args.Handled)
            return;
        Logger.Info("Weh");

        args.Handled = true;
    }
}