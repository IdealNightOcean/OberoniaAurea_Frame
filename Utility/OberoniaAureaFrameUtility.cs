using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;


[StaticConstructorOnStartup]
public static class OberoniaAureaFrameUtility
{
    //是否为玩家派系
    public static bool IsPlayerFaction(this Faction faction)
    {
        return faction?.def.isPlayer ?? false;
    }
    //是否为鼠族派系
    public static bool IsRatkinFaction(this Faction faction)
    {
        if (faction == null)
        {
            return false;
        }
        return faction.def.categoryTag?.Equals("RatkinStory") ?? false;
    }
    //是否是商品
    public static bool IsSiteTraderGood(this Pawn pawn)
    {
        return pawn.ParentHolder is SiteTrader;
    }
    //创建物品
    public static List<Thing> TryGenerateThing(ThingDef def, int count)
    {
        List<Thing> list = [];
        int stackLimit = def.stackLimit;
        int remaining = count;
        while (remaining > 0)
        {
            Thing thing = ThingMaker.MakeThing(def);
            thing.stackCount = Mathf.Min(remaining, stackLimit);
            list.Add(thing);
            remaining -= stackLimit;
        }
        return list;
    }

    public static List<List<Thing>> TryGengrateThingGroup(ThingDef def, int count)
    {
        List<List<Thing>> lists = [];
        int perPodCount = Mathf.Max(1, Mathf.FloorToInt(150 / def.GetStatValueAbstract(StatDefOf.Mass)));
        int remaining = count;
        while (remaining > 0)
        {
            lists.Add(TryGenerateThing(def, Mathf.Min(remaining, perPodCount)));
            remaining -= perPodCount;
        }
        return lists;
    }

    public static Faction RandomFactionOfDef(FactionDef def, bool allowDefeated = false, bool allowTemporary = false)
    {
        Faction faction = Find.FactionManager.AllFactionsListForReading.Where(f => f.def == def && ValidFaction(f)).RandomElementWithFallback(null);
        return faction;

        bool ValidFaction(Faction tf)
        {
            if (tf == null)
            {
                return false;
            }
            if (tf.defeated && !allowDefeated)
            {
                return false;
            }
            if (tf.temporary && !allowTemporary)
            {
                return false;
            }
            return true;
        }
    }

    public static bool CaravanHasAnyThingsOf(Caravan caravan, ThingCategoryDef thingCategoryDef, Func<Thing, bool> validator = null)
    {
        List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravan);
        for (int i = 0; i < list.Count; i++)
        {
            Thing thing = list[i];
            if (thing.def.thingCategories.Contains(thingCategoryDef) && (validator == null || validator(thing)))
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