using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class CaravanArrivalAction_GenerateAndEnter : CaravanArrivalAction
{
    private MapParent mapParent;

    public override string Label => "EnterMap".Translate(mapParent.Label);

    public override string ReportString => "CaravanEntering".Translate(mapParent.Label);

    public CaravanArrivalAction_GenerateAndEnter() { }

    public CaravanArrivalAction_GenerateAndEnter(MapParent mapParent)
    {
        this.mapParent = mapParent;
    }

    public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
    {
        FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
        if (!floatMenuAcceptanceReport)
        {
            return floatMenuAcceptanceReport;
        }

        if (mapParent is not null && mapParent.Tile != destinationTile)
        {
            return false;
        }

        return CanGenerate(mapParent);
    }

    public override void Arrived(Caravan caravan)
    {
        Map map = mapParent.Map;
        if (map is null)
        {
            LongEventHandler.QueueLongEvent(delegate
            {
                Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, mapParent.def.overrideMapSize ?? new IntVec3(200, 1, 200), mapParent.def);

                if (orGenerateMap is not null)
                {
                    EnterMap(caravan, orGenerateMap, mapParent);
                }
            }, "GeneratingMap", doAsynchronously: false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
        }
        else
        {
            EnterMap(caravan, map, mapParent);
        }
    }

    private static void EnterMap(Caravan caravan, Map map, MapParent mapParent)
    {
        CaravanDropInventoryMode dropInventoryMode = map.IsPlayerHome ? CaravanDropInventoryMode.UnloadIndividually : CaravanDropInventoryMode.DoNotDrop;
        bool draftColonists = mapParent.Faction is not null && mapParent.Faction.HostileTo(Faction.OfPlayer);
        if (caravan.IsPlayerControlled || mapParent.Faction == Faction.OfPlayer)
        {
            Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredMap".Translate(mapParent), "LetterCaravanEnteredMap".Translate(caravan.Label, mapParent).CapitalizeFirst(), LetterDefOf.NeutralEvent, caravan.PawnsListForReading);
        }

        CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Edge, dropInventoryMode, draftColonists);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref mapParent, "mapParent");
    }

    public static FloatMenuAcceptanceReport CanGenerate(MapParent mapParent)
    {
        if (mapParent is null || !mapParent.Spawned || mapParent.HasMap)
        {
            return false;
        }

        if (mapParent.EnterCooldownBlocksEntering())
        {
            return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(mapParent.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
        }

        return true;
    }

    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, MapParent mapParent)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanGenerate(mapParent), () => new CaravanArrivalAction_GenerateAndEnter(mapParent), "EnterMap".Translate(mapParent.Label), caravan, mapParent.Tile, mapParent);
    }
}