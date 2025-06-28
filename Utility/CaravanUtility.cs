using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_CaravanUtility
{
    public static bool IsExactTypeCaravan(object caravan)
    {
        if (caravan is null)
        {
            return false;
        }
        if (caravan.GetType() == typeof(Caravan))
        {
            return true;
        }
        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OAFrame_WarningAbnormalCaravan".Translate(), null, destructive: false, title: "OAFrame_WarningAbnormalCaravanTitle".Translate()));
        return false;
    }

    public static bool CaravanHasAnyThingsOfCategory(Caravan caravan, ThingCategoryDef thingCategoryDef, Func<Thing, bool> validator = null)
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
    public static bool CaravanHasEnoughThingsOfCategory(Caravan caravan, ThingCategoryDef thingCategoryDef, int count, Func<Thing, bool> validator = null)
    {
        int num = 0;
        List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravan);
        for (int i = 0; i < list.Count; i++)
        {
            Thing thing = list[i];
            if (thing.def.thingCategories.Contains(thingCategoryDef) && (validator is null || validator(thing)))
            {
                num += thing.stackCount;
            }
        }
        return num >= count;
    }

    public static int RemoveThings(Caravan caravan, ThingDef thingDef, int count)
    {
        int remaining = count;
        List<Thing> takeThings = CaravanInventoryUtility.TakeThings(caravan, delegate (Thing thing)
        {
            if (thingDef != thing.def)
            {
                return 0;
            }
            int takeCount = Mathf.Min(remaining, thing.stackCount);
            remaining -= takeCount;
            return takeCount;
        });
        for (int i = 0; i < takeThings.Count; i++)
        {
            takeThings[i].Destroy();
        }

        return remaining;
    }
}