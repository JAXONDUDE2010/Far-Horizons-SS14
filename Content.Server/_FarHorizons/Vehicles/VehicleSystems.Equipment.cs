using Content.Shared._FarHorizons.Vehicles;
using Content.Shared._FarHorizons.Vehicles.Components;
using Content.Shared.Coordinates;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;

namespace Content.Server._FarHorizons.Vehicle.Equipment;
public sealed partial class VehicleEquipmentSystems : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VehicleModsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RiderComponent, AddRiderActions>(OnAddActions);
        SubscribeLocalEvent<RiderComponent, RemoveRiderActions>(OnRemoveActions);
    }

    private void OnMapInit(Entity<VehicleModsComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.ModSlot = _container.EnsureContainer<Container>(ent.Owner, ent.Comp.ModContainer);

        foreach(var itemProto in ent.Comp.StartingEquipment)
        {
            var item = SpawnAtPosition(itemProto, ent.Owner.ToCoordinates());
            if(HasComp<PointLightComponent>(item))
            {
                _transform.SetParent(item, ent.Owner);
            }
            else
                _container.Insert(item, ent.Comp.ModSlot);

            ent.Comp.SpawnedEquipment.Add(item);
        }
        Dirty(ent.Owner, ent.Comp);
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