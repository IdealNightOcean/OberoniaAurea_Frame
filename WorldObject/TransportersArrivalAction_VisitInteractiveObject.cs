using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

public class TransportersArrivalAction_VisitInteractiveObject : TransportersArrivalAction
{
    private static readonly List<Pawn> tmpPawns = [];
    private static readonly List<Thing> tmpContainedThings = [];

    protected WorldObject_InteractiveBase worldObject;

    private string arrivalMessageKey = "MessageTransportPodsArrived";

    public override bool GeneratesMap => false;

    public TransportersArrivalAction_VisitInteractiveObject() { }

    public TransportersArrivalAction_VisitInteractiveObject(WorldObject_InteractiveBase worldObject)
    {
        this.worldObject = worldObject;
    }

    public TransportersArrivalAction_VisitInteractiveObject(WorldObject_InteractiveBase worldObject, string arrivalMessageKey)
    {
        this.worldObject = worldObject;
        this.arrivalMessageKey = arrivalMessageKey;
    }

    public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
    {
        FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
        if (!floatMenuAcceptanceReport)
        {
            return floatMenuAcceptanceReport;
        }
        return CanFormCaravanAt(pods, destinationTile);
    }

    public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
    {
        tmpPawns.Clear();
        for (int i = 0; i < transporters.Count; i++)
        {
            ThingOwner innerContainer = transporters[i].innerContainer;
            for (int num = innerContainer.Count - 1; num >= 0; num--)
            {
                if (innerContainer[num] is Pawn pawn)
                {
                    tmpPawns.Add(pawn);
                    innerContainer.Remove(pawn);
                }
            }
        }
        if (!GenWorldClosest.TryFindClosestPassableTile(tile, out PlanetTile foundTile))
        {
            foundTile = tile;
        }
        Caravan caravan = CaravanMaker.MakeCaravan(tmpPawns, Faction.OfPlayer, foundTile, addToWorldPawnsIfNotAlready: true);
        if (transporters.IsShuttle())
        {
            CaravanInventoryUtility.GiveThing(caravan, transporters[0].RemoveShuttle());
        }
        for (int j = 0; j < transporters.Count; j++)
        {
            tmpContainedThings.Clear();
            tmpContainedThings.AddRange(transporters[j].innerContainer);
            for (int k = 0; k < tmpContainedThings.Count; k++)
            {
                transporters[j].innerContainer.Remove(tmpContainedThings[k]);
                CaravanInventoryUtility.GiveThing(caravan, tmpContainedThings[k]);
            }
        }
        tmpPawns.Clear();
        tmpContainedThings.Clear();
        Messages.Message(arrivalMessageKey.Translate(), caravan, MessageTypeDefOf.TaskCompletion);

        worldObject?.Notify_CaravanArrived(caravan);
    }

    public static bool CanFormCaravanAt(IEnumerable<IThingHolder> pods, PlanetTile tile)
    {
        if (TransportersArrivalActionUtility.AnyPotentialCaravanOwner(pods, Faction.OfPlayer) && !Find.World.Impassable(tile))
        {
            return tile.LayerDef.canFormCaravans;
        }
        return false;
    }


    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Action<PlanetTile, TransportersArrivalAction> launchAction, IEnumerable<IThingHolder> pods, WorldObject_InteractiveBase worldObject)
    {
        foreach (FloatMenuOption floatMenuOption in TransportersArrivalActionUtility.GetFloatMenuOptions(acceptanceReportGetter: () => CanFormCaravanAt(pods, worldObject.Tile),
                                                                                                         arrivalActionGetter: () => new TransportersArrivalAction_VisitInteractiveObject(worldObject),
                                                                                                         label: "VisitSettlement".Translate(worldObject.Label),
                                                                                                         launchAction: launchAction,
                                                                                                         destinationTile: worldObject.Tile))
        {
            yield return floatMenuOption;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref worldObject, "worldObject");
        Scribe_Values.Look(ref arrivalMessageKey, "arrivalMessageKey", "MessageTransportPodsArrived");
    }
}
