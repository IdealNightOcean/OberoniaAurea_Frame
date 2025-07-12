using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_CaravanUtility
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsExactTypeCaravan<T>(T caravan) where T : Caravan
    {
        if (caravan is not null && caravan.GetType() == typeof(Caravan))
        {
            return true;
        }

        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OAFrame_WarningAbnormalCaravan".Translate(), null, destructive: false, title: "OAFrame_WarningAbnormalCaravanTitle".Translate()));
        return false;
    }

    public static bool CaravanHasAnyThingsOfCategory(Caravan caravan, ThingCategoryDef thingCategoryDef, Predicate<Thing> validator = null)
    {
        List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravan);
        for (int i = 0; i < list.Count; i++)
        {
            Thing thing = list[i];
            if (thing.def.IsWithinCategory(thingCategoryDef) && (validator is null || validator(thing)))
            {
                return true;
            }
        }
        return false;
    }

    public static bool CaravanHasEnoughThingsOfCategory(Caravan caravan, ThingCategoryDef thingCategoryDef, int count, Predicate<Thing> validator = null)
    {
        int num = 0;
        List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravan);
        for (int i = 0; i < list.Count; i++)
        {
            Thing thing = list[i];
            if (thing.def.thingCategories.Contains(thingCategoryDef) && (validator is null || validator(thing)))
            {
                num += thing.stackCount;
                if (num >= count)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static int RemoveThings(Caravan caravan, ThingDef thingDef, int count)
    {
        int remaining = count;
        List<Thing> takeThings = CaravanInventoryUtility.TakeThings(caravan, GetTakeThingCount);
        for (int i = 0; i < takeThings.Count; i++)
        {
            takeThings[i].Destroy();
        }

        return remaining;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int GetTakeThingCount(Thing thing)
        {
            if (remaining <= 0 || thingDef != thing.def)
            {
                return 0;
            }
            int takeCount = Mathf.Min(remaining, thing.stackCount);
            remaining -= takeCount;
            return takeCount;
        }
    }
}