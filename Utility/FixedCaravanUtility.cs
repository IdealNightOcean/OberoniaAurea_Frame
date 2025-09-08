using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_FixedCaravanUtility
{
    private static readonly List<Thing> TempInventoryItems = [];
    private static readonly List<Thing> TempAddedItems = [];
    private static readonly List<Pawn> TempPawns = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedCaravan CreateFixedCaravan(Caravan caravan)
    {
        return CreateFixedCaravan(caravan, OAFrameDefOf.OAFrame_FixedCaravan);
    }

    public static FixedCaravan CreateFixedCaravan(Caravan caravan, WorldObjectDef def)
    {
        FixedCaravan fixedCaravan = (FixedCaravan)WorldObjectMaker.MakeWorldObject(def);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedCaravan CreateFixedCaravan(Caravan caravan, WorldObject worldObject)
    {
        return CreateFixedCaravan(caravan, OAFrameDefOf.OAFrame_FixedCaravan, worldObject);
    }

    public static FixedCaravan CreateFixedCaravan(Caravan caravan, WorldObjectDef def, WorldObject worldObject)
    {
        if (worldObject is null)
        {
            Log.Error($"Failed to convert Caravan {caravan} to FixedCaravan due to a null worldObject.");
            return null;
        }
        FixedCaravan fixedCaravan = CreateFixedCaravan(caravan, def);
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

        fixedCaravan.PostConvertToCaravan(caravan);
        fixedCaravan.Destroy();
        TempPawns.Clear();
        return caravan;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GiveThings(FixedCaravan fixedCaravan, IEnumerable<Thing> things)
    {
        foreach (Thing t in things)
        {
            GiveThing(fixedCaravan, t);
        }
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

    public static List<Thing> TakeThingsOfDef(FixedCaravan fixedCaravan, ThingDef thingDef, int count, out int actualTakeCount)
    {
        actualTakeCount = 0;
        if (fixedCaravan is null || thingDef is null || count <= 0)
        {
            return [];
        }

        List<Thing> caravanInventory = AllInventoryItems(fixedCaravan);

        int remaining = count;
        int takeCount;
        List<Thing> takeThings = [];
        Thing item;
        for (int i = caravanInventory.Count - 1; i >= 0; i--)
        {
            item = caravanInventory[i];
            if (item.def != thingDef)
            {
                continue;
            }

            takeCount = Mathf.Min(remaining, item.stackCount);
            takeThings.Add(item.holdingOwner.Take(item, takeCount));
            if ((remaining -= takeCount) <= 0)
            {
                break;
            }
        }

        actualTakeCount = count - remaining;
        return takeThings;
    }
    public static List<Thing> TakeThingsOfDef(FixedCaravan fixedCaravan, ThingDef thingDef, int count, Predicate<Thing> validator, out int actualTakeCount)
    {
        if (validator is null)
        {
            return TakeThingsOfDef(fixedCaravan, thingDef, count, out actualTakeCount);
        }

        actualTakeCount = 0;
        if (fixedCaravan is null || thingDef is null || count <= 0)
        {
            return [];
        }

        List<Thing> caravanInventory = AllInventoryItems(fixedCaravan);

        int remaining = count;
        int takeCount;
        List<Thing> takeThings = [];
        Thing item;
        for (int i = caravanInventory.Count - 1; i >= 0; i--)
        {
            item = caravanInventory[i];
            if (item.def != thingDef || !validator(item))
            {
                continue;
            }

            takeCount = Mathf.Min(remaining, item.stackCount);
            takeThings.Add(item.holdingOwner.Take(item, takeCount));
            if ((remaining -= takeCount) <= 0)
            {
                break;
            }
        }

        actualTakeCount = count - remaining;
        return takeThings;
    }

    /// <returns>实际移除数</returns>
    public static int RemoveThingsOfDef(this FixedCaravan fixedCaravan, ThingDef thingDef, int count)
    {
        List<Thing> takeThings = TakeThingsOfDef(fixedCaravan, thingDef, count, out int actualTakeCount);
        for (int i = takeThings.Count - 1; i >= 0; i--)
        {
            takeThings[i].Destroy();
        }

        return actualTakeCount;
    }

    /// <returns>实际移除数</returns>
    public static int RemoveThingsOfDef(this FixedCaravan fixedCaravan, ThingDef thingDef, int count, Predicate<Thing> validator)
    {
        if (validator is null)
        {
            return RemoveThingsOfDef(fixedCaravan, thingDef, count);
        }

        List<Thing> takeThings = TakeThingsOfDef(fixedCaravan, thingDef, count, validator, out int actualTakeCount);
        for (int i = takeThings.Count - 1; i >= 0; i--)
        {
            takeThings[i].Destroy();
        }

        return actualTakeCount;
    }
}