using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_CaravanUtility
{
    public static bool IsExactTypeCaravan(object caravan)
    {
        if (caravan == null)
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

    public static bool CaravanHasAnyThingsOf(Caravan caravan, ThingCategoryDef thingCategoryDef, Func<Thing, bool> validator = null)
    {
        List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravan);
        for (int i = 0; i < list.Count; i++)
        {
            Thing thing = list[i];
            if (thing.def.IsWithinCategory(thingCategoryDef) && (validator == null || validator(thing)))
            {
                return true;
            }
        }
        return false;
    }
    public static bool CaravanHasEnoughThingsOf(Caravan caravan, ThingCategoryDef thingCategoryDef, int count, Func<Thing, bool> validator = null)
    {
        int num = 0;
        List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravan);
        for (int i = 0; i < list.Count; i++)
        {
            Thing thing = list[i];
            if (thing.def.thingCategories.Contains(thingCategoryDef) && (validator == null || validator(thing)))
            {
                num += thing.stackCount;
            }
        }
        return num >= count;
    }
}