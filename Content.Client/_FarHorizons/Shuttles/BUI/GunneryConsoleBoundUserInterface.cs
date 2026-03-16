using System.Numerics;
using Content.Client._FarHorizons.Shuttles.UI;
using Content.Shared._FarHorizons.Shuttles;
using Content.Shared.Shuttles.BUIStates;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Map;

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
        _window.FireButtonPressed += OnFireButtonPressed;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        switch (state)
        {
            case NavBoundUserInterfaceState navState:
                _window?.SetShuttle(EntMan.GetCoordinates(navState.State.Coordinates)?.EntityId);
                _window?.UpdateState(navState.State);
                break;
            case GunneryConsoleBuiState gunState:
                _window?.SetShuttle(EntMan.GetCoordinates(gunState.State.Coordinates)?.EntityId);
                _window?.Update(gunState);
                _window?.UpdateState(gunState.State);
                break;
        }
    }

    private void OnFireButtonPressed(NetCoordinates position, List<NetEntity> turretEntities) 
        => SendMessage(new GunneryConsoleFireActionMessage(position, turretEntities));
}
