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
    public static bool IsExactTypeCaravan<T>(this T caravan) where T : Caravan
    {
        if (caravan is not null && caravan.GetType() == typeof(Caravan))
        {
            return true;
        }

        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OAFrame_WarningAbnormalCaravan".Translate(), null, destructive: false, title: "OAFrame_WarningAbnormalCaravanTitle".Translate()));
        return false;
    }

    public static bool HasAnyThingOfCategory(this Caravan caravan, ThingCategoryDef thingCategoryDef, Predicate<Thing> validator = null)
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

    public static bool HasEnoughThingsOfCategory(this Caravan caravan, ThingCategoryDef thingCategoryDef, int count, Predicate<Thing> validator = null)
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

    public static List<Thing> TakeThingsOfDef(Caravan caravan, ThingDef thingDef, int count, out int actualTakeCount)
    {
        actualTakeCount = 0;
        if (caravan is null || thingDef is null || count <= 0)
        {
            return [];
        }

        List<Thing> caravanInventory = CaravanInventoryUtility.AllInventoryItems(caravan);

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

    public static List<Thing> TakeThingsOfDef(Caravan caravan, ThingDef thingDef, int count, Predicate<Thing> validator, out int actualTakeCount)
    {
        if (validator is null)
        {
            return TakeThingsOfDef(caravan, thingDef, count, out actualTakeCount);
        }

        actualTakeCount = 0;
        if (caravan is null || thingDef is null || count <= 0)
        {
            return [];
        }

        List<Thing> caravanInventory = CaravanInventoryUtility.AllInventoryItems(caravan);

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

    /// <returns>实际移除数</returns>
    public static int RemoveThingsOfDef(this Caravan caravan, ThingDef thingDef, int count)
    {
        List<Thing> takeThings = TakeThingsOfDef(caravan, thingDef, count, out int actualTakeCount);
        for (int i = takeThings.Count - 1; i >= 0; i--)
        {
            takeThings[i].Destroy();
        }

        return actualTakeCount;
    }

    /// <returns>实际移除数</returns>
    public static int RemoveThingsOfDef(this Caravan caravan, ThingDef thingDef, int count, Predicate<Thing> validator)
    {
        List<Thing> takeThings = TakeThingsOfDef(caravan, thingDef, count, validator, out int actualTakeCount);
        for (int i = takeThings.Count - 1; i >= 0; i--)
        {
            takeThings[i].Destroy();
        }

        return actualTakeCount;
    }
}