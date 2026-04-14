using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_CaravanUtility
{
    /// <summary>
    /// 检查是否为精确类型的远行队。
    /// </summary>
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

    /// <summary>
    /// 检查远行队是否有指定类别(thingCategoryDef)的物品。
    /// </summary>
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

    /// <summary>
    /// 检查远行队是否有足够数量的指定类别(thingCategoryDef)物品。
    /// </summary>
    public static bool HasEnoughThingsOfCategory(this Caravan caravan, ThingCategoryDef thingCategoryDef, int count, Predicate<Thing> validator = null)
    {
        int num = 0;
        List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravan);
        if (validator is null)
        {
            foreach (Thing thing in list)
            {
                ThingDef thingDef = thing.def;
                if (thingDef.thingCategories is not null && thingDef.thingCategories.Contains(thingCategoryDef))
                {
                    num += thing.stackCount;
                    if (num >= count)
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            foreach (Thing thing in list)
            {
                ThingDef thingDef = thing.def;
                if (thingDef.thingCategories is null)
                {
                    continue;
                }
                if (thingDef.thingCategories.Contains(thingCategoryDef) && validator(thing))
                {
                    num += thing.stackCount;
                    if (num >= count)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 获取远行队中指定def的数量。
    /// </summary>
    public static int GetCountOfThingDef(this Caravan caravan, ThingDef thingDef, Predicate<Thing> validator = null)
    {
        int totalCount = 0;
        List<Thing> caravanInventory = CaravanInventoryUtility.AllInventoryItems(caravan);

        Thing item;
        if (validator is null)
        {
            for (int i = 0; i < caravanInventory.Count; i++)
            {
                item = caravanInventory[i];
                if (item.def == thingDef)
                {
                    totalCount += item.stackCount;
                }
            }
        }
        else
        {
            for (int i = 0; i < caravanInventory.Count; i++)
            {
                item = caravanInventory[i];
                if (item.def == thingDef && validator(item))
                {
                    totalCount += item.stackCount;
                }
            }
        }
        return totalCount;
    }

    /// <summary>
    /// 从远行队取出指定def的物品。
    /// </summary>
    public static List<Thing> TakeThingsOfDef(Caravan caravan, ThingDef thingDef, int count, out int actualTakeCount, Predicate<Thing> validator = null)
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
        if (validator is null)
        {
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
        }
        else
        {
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
        }

        actualTakeCount = count - remaining;
        return takeThings;
    }

    /// <summary>
    /// 移除远行队中指定物品def的物品。
    /// </summary>
    /// <returns>实际移除数</returns>
    public static int RemoveThingsOfDef(this Caravan caravan, ThingDef thingDef, int count, Predicate<Thing> validator = null)
    {
        List<Thing> takeThings = TakeThingsOfDef(caravan, thingDef, count, out int actualTakeCount, validator);
        for (int i = takeThings.Count - 1; i >= 0; i--)
        {
            takeThings[i].Destroy();
        }

        return actualTakeCount;
    }
}