using Content.Client._FarHorizons.Shuttles.UI;
using Content.Shared.Shuttles.BUIStates;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._FarHorizons.Shuttles.BUI;

[UsedImplicitly]
public sealed class GunneryConsoleBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    
    [ViewVariables]
    private GunneryConsoleWindow? _window;

    public GunneryConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<GunneryConsoleWindow>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not NavBoundUserInterfaceState cState)
            return;

        var coordinates = _entManager.GetCoordinates(cState.State.Coordinates);
        _window?.SetShuttle(coordinates?.EntityId);
        _window?.UpdateState(cState.State);
    }
}
