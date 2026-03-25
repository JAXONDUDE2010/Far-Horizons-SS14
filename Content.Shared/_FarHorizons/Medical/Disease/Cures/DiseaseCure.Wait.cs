using Content.Shared.Medical.Disease.Prototypes;
using Content.Shared.Medical.Disease.Components;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared.Medical.Disease.Systems;

namespace Content.Shared.Medical.Disease.Cures;

[Serializable, NetSerializable]
public sealed partial class CureWait : CureStep
{
    /// <summary>
    /// Ticks since infection required before curing can occur.
    /// </summary>
    [DataField]
    public int RequiredTicks { get; private set; } = 90;
}

public sealed partial class CureWait
{
    /// <summary>
    /// Cures the disease after the infection has lasted a configured duration.
    /// </summary>
    public override bool OnCure(EntityUid uid, DiseaseData disease)
    {
        var _entitySysManager = IoCManager.Resolve<IEntitySystemManager>();
        var _cureSystem = _entitySysManager.GetEntitySystem<SharedDiseaseCureSystem>();
        var _random = IoCManager.Resolve<IRobustRandom>();

        if (RequiredTicks <= 0f)
            return false;

        var state = _cureSystem.GetState(uid, disease.Id, this);
        state.Ticker++;
        if (state.Ticker < RequiredTicks)
            return false;

        if (_random.Prob(CureChance))
        {
            state.Ticker = 0;
            return true;
        }

        state.Ticker = 0;
        return false;
    }

    public override IEnumerable<string> BuildDiagnoserLines(IPrototypeManager prototypes)
    {
        var defaultTickSeconds = new DiseaseCarrierComponent().TickDelay.TotalSeconds;
        var seconds = RequiredTicks * defaultTickSeconds;
        yield return Loc.GetString("diagnoser-cure-time", ("time", seconds));
    }
}
