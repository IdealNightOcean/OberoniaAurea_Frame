using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_FixedCaravanUtility
{
    private static readonly List<Thing> TempInventoryItems = [];
    private static readonly List<Thing> TempAddedItems = [];
    private static readonly List<Pawn> TempPawns = [];

    public static bool IsFixedCaravanMember(this Pawn pawn)
    {
        return pawn.GetFixedCaravan() is not null;
    }
    public static FixedCaravan GetFixedCaravan(this Pawn pawn)
    {
        return pawn.ParentHolder as FixedCaravan;
    }
    public static List<Thing> AllInventoryItems(FixedCaravan fixedCaravan)
    {
        TempInventoryItems.Clear();
        List<Pawn> allPawnsForReading = fixedCaravan.PawnsListForReading;
        for (int i = 0; i < allPawnsForReading.Count; i++)
        {
            Pawn pawn = allPawnsForReading[i];
            for (int j = 0; j < pawn.inventory.innerContainer.Count; j++)
            {
                Thing item = pawn.inventory.innerContainer[j];
                TempInventoryItems.Add(item);
            }
        }
        return TempInventoryItems;
    }

    public static FixedCaravan CreateFixedCaravan(Caravan caravan)
    {
        FixedCaravan fixedCaravan = (FixedCaravan)WorldObjectMaker.MakeWorldObject(OAFrameDefOf.OAFrame_FixedCaravan);
        fixedCaravan.curName = caravan.Name;
        fixedCaravan.Tile = caravan.Tile;
        fixedCaravan.SetFaction(caravan.Faction);

        try
        {
            ConvertToFixedCaravan(caravan, fixedCaravan);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to convert Caravan {caravan} to a FixedCaravan: " + ex.Message);
            fixedCaravan.Destroy();
            return null;
        }

        return fixedCaravan;
    }

    public static FixedCaravan CreateFixedCaravan(Caravan caravan, WorldObject_InteractiveWithFixedCarvanBase worldObject)
    {
        if (worldObject is null)
        {
            Log.Error($"Failed to convert Caravan {caravan} to a FixedCaravan: WorldObject is null");
            return null;
        }
        FixedCaravan fixedCaravan = CreateFixedCaravan(caravan);
        fixedCaravan.SetAssociatedWorldObject(worldObject);
        return fixedCaravan;
    }

    private static void ConvertToFixedCaravan(Caravan caravan, FixedCaravan fixedCaravan, bool addToWorldPawnsIfNotAlready = true)
    {
        TempPawns.Clear();
        TempPawns.AddRange(caravan.PawnsListForReading);
        for (int i = 0; i < TempPawns.Count; i++)
        {
            Pawn pawn = TempPawns[i];
            if (pawn.Dead)
            {
                Log.Warning("Tried to form a caravan with a dead pawn " + pawn);
                continue;
            }
            if (!fixedCaravan.ContainsPawn(pawn))
            {
                caravan.RemovePawn(pawn);
                fixedCaravan.AddPawn(pawn, addToWorldPawnsIfNotAlready);
            }
            if (addToWorldPawnsIfNotAlready && !pawn.IsWorldPawn())
            {
                Find.WorldPawns.PassToWorld(pawn);
            }
        }
        TempPawns.Clear();

        GivePawnsOrThings(fixedCaravan, caravan.AllThings.ToList());
        caravan.Destroy();

    }

    public static Caravan ConvertToCaravan(FixedCaravan fixedCaravan)
    {
        TempPawns.Clear();
        TempPawns.AddRange(fixedCaravan.PawnsListForReading);
        fixedCaravan.RemoveAllPawns();
        Caravan caravan = CaravanMaker.MakeCaravan(TempPawns, fixedCaravan.Faction, fixedCaravan.Tile, addToWorldPawnsIfNotAlready: true);
        if (Find.WorldSelector.IsSelected(fixedCaravan))
        {
            Find.WorldSelector.Select(caravan, playSound: false);
        }
        fixedCaravan.Destroy();
        TempPawns.Clear();
        return caravan;
    }

    public static void GiveThing(FixedCaravan fixedCaravan, Thing thing)
    {
        if (AllInventoryItems(fixedCaravan).Contains(thing))
        {
            Log.Error(string.Concat("Tried to give the same item twice (", thing, ") to a caravan (", fixedCaravan, ")."));
            return;
        }
        Pawn pawn = CaravanInventoryUtility.FindPawnToMoveInventoryTo(thing, fixedCaravan.PawnsListForReading, null);
        if (pawn is null)
        {
            Log.Error($"Failed to give item {thing} to caravan {fixedCaravan}; item was lost");
            thing.Destroy();
        }
        else if (!pawn.inventory.innerContainer.TryAdd(thing))
        {
            Log.Error($"Failed to give item {thing} to caravan {fixedCaravan}; item was lost");
            thing.Destroy();
        }
    }
    public static void GivePawnsOrThings(FixedCaravan fixedCaravan, List<Thing> things)
    {
        TempAddedItems.Clear();
        TempAddedItems.AddRange(things);
        for (int i = 0; i < TempAddedItems.Count; i++)
        {
            fixedCaravan.AddPawnOrItem(TempAddedItems[i]);
        }
        TempAddedItems.Clear();
    }

}