using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class TransportersArrivalAction_VisitEnterableMap : TransportersArrivalAction
{
    private MapParent_Enterable mapParent;

    private PawnsArrivalModeDef arrivalMode;

    public override bool GeneratesMap => true;

    public TransportersArrivalAction_VisitEnterableMap() { }

    public TransportersArrivalAction_VisitEnterableMap(MapParent_Enterable mapParent, PawnsArrivalModeDef arrivalMode)
    {
        this.mapParent = mapParent;
        this.arrivalMode = arrivalMode;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref mapParent, "mapParent");
        Scribe_Defs.Look(ref arrivalMode, "arrivalMode");
    }

    public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
    {
        if (mapParent is not null && mapParent.Tile != destinationTile)
        {
            return false;
        }

        return CanVisit(pods, mapParent);
    }

    public override bool ShouldUseLongEvent(List<ActiveTransporterInfo> pods, PlanetTile tile)
    {
        return !mapParent.HasMap;
    }

    public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
    {
        Thing lookTarget = TransportersArrivalActionUtility.GetLookTarget(transporters);
        Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, mapParent.def.overrideMapSize ?? new IntVec3(200, 1, 200), mapParent.def);

        if (!mapParent.HasMap)
        {
            Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(orGenerateMap.mapPawns.AllPawns, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true);
        }
        if (transporters.IsShuttle())
        {
            Messages.Message("MessageShuttleArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
        }
        else
        {
            Messages.Message("MessageTransportPodsArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
        }
        arrivalMode.Worker.TravellingTransportersArrived(transporters, orGenerateMap);
    }

    public static FloatMenuAcceptanceReport CanVisit(IEnumerable<IThingHolder> pods, MapParent_Enterable mapParent)
    {
        if (mapParent is null || !mapParent.Spawned)
        {
            return false;
        }
        if (mapParent.HasMap)
        {
            return true;
        }

        if (!TransportersArrivalActionUtility.AnyNonDownedColonist(pods))
        {
            return false;
        }
        return mapParent.CanEnterMap(pods);
    }

    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Action<PlanetTile, TransportersArrivalAction> launchAction, IEnumerable<IThingHolder> pods, MapParent_Enterable mapParent)
    {
        foreach (FloatMenuOption edgeMenuOption in TransportersArrivalActionUtility.GetFloatMenuOptions(acceptanceReportGetter: () => CanVisit(pods, mapParent),
                                                                                                        arrivalActionGetter: () => new TransportersArrivalAction_VisitEnterableMap(mapParent, PawnsArrivalModeDefOf.EdgeDrop),
                                                                                                        label: "DropAtEdge".Translate(),
                                                                                                        launchAction: launchAction,
                                                                                                        destinationTile: mapParent.Tile,
                                                                                                        uiConfirmationCallback: UIConfirmationCallback))
        {
            yield return edgeMenuOption;
        }
        foreach (FloatMenuOption centerMenuOption in TransportersArrivalActionUtility.GetFloatMenuOptions(acceptanceReportGetter: () => CanVisit(pods, mapParent),
                                                                                                          arrivalActionGetter: () => new TransportersArrivalAction_VisitEnterableMap(mapParent, PawnsArrivalModeDefOf.CenterDrop),
                                                                                                          label: "DropInCenter".Translate(),
                                                                                                          launchAction: launchAction,
                                                                                                          destinationTile: mapParent.Tile,
                                                                                                          uiConfirmationCallback: UIConfirmationCallback))
        {
            yield return centerMenuOption;
        }
        void UIConfirmationCallback(Action action)
        {
            if (ModsConfig.OdysseyActive && mapParent.Tile.LayerDef == PlanetLayerDefOf.Orbit)
            {
                TaggedString text = "OrbitalWarning".Translate();
                text += string.Format("\n\n{0}", "LaunchToConfirmation".Translate());
                Find.WindowStack.Add(new Dialog_MessageBox(text, null, action, "Cancel".Translate(), delegate
                {
                }, null, buttonADestructive: true));
            }
            else
            {
                action();
            }
        }
    }
}