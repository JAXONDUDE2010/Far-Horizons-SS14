using Content.Shared._FarHorizons.Vehicles;
using Content.Shared._FarHorizons.Vehicles.Components;

namespace Content.Server._FarHorizons.Vehicle.Equipment;
public sealed partial class VehicleEquipmentSystems : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RiderComponent, AddRiderActions>(OnAddActions);
        SubscribeLocalEvent<RiderComponent,RemoveRiderActions>(OnRemoveActions);
    }

    private void OnAddActions(Entity<RiderComponent> ent, ref AddRiderActions args)
    {
        Log.Info("Weh");
        Log.Info($"{args.Rider}");
    }
    private void OnRemoveActions(Entity<RiderComponent> ent, ref RemoveRiderActions args)
    {
        Log.Info("Hew");
        Log.Info($"{args.Rider}");
    }
}