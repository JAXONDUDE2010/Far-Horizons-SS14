using Robust.Client.UserInterface;

namespace Content.Client._FarHorizons.Medical.Disease.UI;

public sealed class VaccinatorBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private VaccinatorMenu? _menu;
    public VaccinatorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {}

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<VaccinatorMenu>();
        _menu.SetEntity(Owner);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if(_menu == null)
            return;
    }
}