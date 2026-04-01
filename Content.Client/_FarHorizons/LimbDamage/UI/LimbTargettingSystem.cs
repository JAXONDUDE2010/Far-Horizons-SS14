using Content.Shared._FarHorizons.LimbDamage.Components;
using Content.Shared.Body;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.LimbDamage.UI;

public sealed class LimbTargettingSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;

    public Action<ProtoId<OrganCategoryPrototype>>? LocalTargetUpdated;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LimbTargettingComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnState(Entity<LimbTargettingComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (ent != _player.LocalEntity) return;
        LocalTargetUpdated?.Invoke(ent.Comp.Target);
    }

    public void SetTarget(ProtoId<OrganCategoryPrototype> target) => 
        RaiseNetworkEvent(new ChangeLimbTargetMessage(target));
}