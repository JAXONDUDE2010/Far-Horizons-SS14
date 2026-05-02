using Content.Shared.Examine;
using Content.Shared.Trigger.Components.Effects;

namespace Content.Server._FarHorizons.Silicons.HumanoidEMP;

public sealed partial class HumanoidEMPSystem
{
    public void InitializeInspect()
    {
        SubscribeLocalEvent<EmpOnTriggerComponent, ExaminedEvent>(OnEmpGrenadeExamine);
    }

    private void OnEmpGrenadeExamine(Entity<EmpOnTriggerComponent> ent, ref ExaminedEvent args)
    {
        if (args.IsInDetailsRange)
            args.PushText(Loc.GetString("emp-grenade-strength-description", ("empStrength", ent.Comp.Strength)), 10);
    }
}