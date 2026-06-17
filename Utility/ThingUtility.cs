using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 物品工具类
/// </summary>
public static class OAFrame_ThingUtility
{
    /// <summary>
    /// 生成物品
    /// </summary>
    /// <remarks>堆叠超上限不拆分</remarks>
    public static Thing GenerateThing(ThingMakeInfo thingMakeInfo)
    {
        return GenerateThing(thingMakeInfo.ThingDef, thingMakeInfo.Count, thingMakeInfo.Quality, thingMakeInfo.StuffDef);
    }

    /// <summary>
    /// 生成物品
    /// </summary>
    /// <remarks>堆叠超上限不拆分</remarks>
    public static Thing GenerateThing(ThingDef def, int count, QualityCategory? quality = null, ThingDef stuffDef = null)
    {
        Thing thing = ThingMaker.MakeThing(def, stuffDef);
        if (quality.HasValue)
        {
            thing.TryGetComp<CompQuality>()?.SetQuality(quality.Value, ArtGenerationContext.Outsider);
        }
        thing.stackCount = count;
        return thing;
    }

    /// <summary>
    /// 生成物品并按堆叠上限自动拆分，以迭代形式返回物品序列
    /// </summary>
    public static IEnumerable<Thing> GenerateThingsSplitByStack(ThingMakeInfo thingMakeInfo)
    {
        return GenerateThingsSplitByStack(thingMakeInfo.ThingDef, thingMakeInfo.Count, thingMakeInfo.Quality, thingMakeInfo.StuffDef);
    }

    /// <summary>
    /// 生成物品并按堆叠上限自动拆分，以迭代形式返回物品序列
    /// </summary>
    public static IEnumerable<Thing> GenerateThingsSplitByStack(ThingDef def, int count, QualityCategory? quality = null, ThingDef stuffDef = null)
    {
        int stackLimit = def.stackLimit;
        int remaining = count;
        bool hasQuality = quality.HasValue;
        QualityCategory qualityValue = hasQuality ? quality.Value : QualityCategory.Normal;

        while (remaining > 0)
        {
            Thing thing = ThingMaker.MakeThing(def, stuffDef);
            if (hasQuality)
            {
                thing.TryGetComp<CompQuality>()?.SetQuality(qualityValue, ArtGenerationContext.Outsider);
            }
            int curStackCount = Mathf.Min(remaining, stackLimit);
            thing.stackCount = curStackCount;

            remaining -= curStackCount;
            yield return thing;
        }
    }

    /// <summary>
    /// 生成物品并按堆叠上限自动拆分，直接返回拆分后的物品列表
    /// </summary>
    public static IEnumerable<Thing> GenerateThingListSplitByStack(ThingMakeInfo thingMakeInfo)
    {
        return GenerateThingListSplitByStack(thingMakeInfo.ThingDef, thingMakeInfo.Count, thingMakeInfo.Quality, thingMakeInfo.StuffDef);
    }

    /// <summary>
    /// 生成物品并按堆叠上限自动拆分，直接返回拆分后的物品列表
    /// </summary>
    public static List<Thing> GenerateThingListSplitByStack(ThingDef def, int count, QualityCategory? quality = null, ThingDef stuffDef = null)
    {
        List<Thing> list = [];
        int stackLimit = def.stackLimit;
        int remaining = count;
        bool hasQuality = quality.HasValue;
        QualityCategory qualityValue = hasQuality ? quality.Value : QualityCategory.Normal;

        while (remaining > 0)
        {
            Thing thing = ThingMaker.MakeThing(def, stuffDef);
            if (hasQuality)
            {
                thing.TryGetComp<CompQuality>()?.SetQuality(qualityValue, ArtGenerationContext.Outsider);
            }
            int curStackCount = Mathf.Min(remaining, stackLimit);
            thing.stackCount = curStackCount;

            remaining -= curStackCount;
            list.Add(thing);
        }
        return list;
    }

    /// <summary>
    /// 给予玩家单个物品
    /// </summary>
    /// <returns>是否成功给予</returns>
    public static bool GiveThingToPlayer(Thing thing, IThingHolder thingHolder)
    {
        if (thing is null || thingHolder is null)
            return false;

        if (thingHolder is Map map)
        {
            IntVec3 spawnCell = DropCellFinder.TradeDropSpot(map);
            if (!spawnCell.IsValid)
                CellFinder.TryRandomClosewalkCellNear(map.Center, map, 100, out spawnCell);
            if (!spawnCell.IsValid)
                spawnCell = map.Center;

            GenPlace.TryPlaceThing(thing, spawnCell, map, ThingPlaceMode.Near);

            return true;
        }

        if (thingHolder is Caravan caravan)
        {
            CaravanInventoryUtility.GiveThing(caravan, thing);
            return true;
        }

        if (thingHolder is FixedCaravan fixedCaravan)
        {
            OAFrame_FixedCaravanUtility.GiveThing(fixedCaravan, thing);
            return true;
        }

        Log.Error($"无法给予物品，未定义的目标类型{thingHolder.GetType()}");
        return false;
    }

    /// <summary>
    /// 给予玩家多个物品
    /// </summary>
    /// <returns>是否成功给予</returns>
    public static bool GiveThingsToPlayer(IEnumerable<Thing> things, IThingHolder thingHolder)
    {
        if (things is null || thingHolder is null)
            return false;

        if (thingHolder is Map map)
        {
            IntVec3 spawnCell = DropCellFinder.TradeDropSpot(map);
            if (!spawnCell.IsValid)
                CellFinder.TryRandomClosewalkCellNear(map.Center, map, 100, out spawnCell);
            if (!spawnCell.IsValid)
                spawnCell = map.Center;

            foreach (Thing t in things)
            {
                GenPlace.TryPlaceThing(t, spawnCell, map, ThingPlaceMode.Near);
            }
            return true;
        }

        if (thingHolder is Caravan caravan)
        {
            OAFrame_CaravanUtility.GiveThings(caravan, things);
            return true;
        }

        if (thingHolder is FixedCaravan fixedCaravan)
        {
            OAFrame_FixedCaravanUtility.GiveThings(fixedCaravan, things);
            return true;
        }

        Log.Error($"无法给予物品，未定义的目标类型{thingHolder.GetType()}");
        return false;
    }

    /// <summary>
    /// 检查容器中是否有足够数量的指定物品
    /// </summary>
    /// <returns>是否有足够数量的物品</returns>
    public static bool HasEnoughThingsOfDef(IThingHolder thingHolder, ThingDef thingDef, int count, Predicate<Thing> validator = null)
    {
        if (thingHolder is null)
            return false;

        if (count <= 0)
            return true;

        if (thingHolder is Map map)
            return OAFrame_MapUtility.HasEnoughThingsOfDef(map, thingDef, count, validator);

        if (thingHolder is Caravan caravan)
            if (validator is null)
                return CaravanInventoryUtility.HasThings(caravan, thingDef, count);
            else
                return CaravanInventoryUtility.HasThings(caravan, thingDef, count, (t) => validator(t));

        if (thingHolder is FixedCaravan fixedCaravan)
            return OAFrame_FixedCaravanUtility.HasThingsOfDef(fixedCaravan, thingDef, count, validator);

        ThingOwner directlyHeldThings = thingHolder.GetDirectlyHeldThings();
        bool hasValidator = validator is not null;
        int totalCount = 0;
        for (int i = directlyHeldThings.Count - 1; i >= 0; i--)
        {
            Thing item = directlyHeldThings[i];
            if (item.def != thingDef && (hasValidator && !validator(item)))
                continue;

            totalCount += item.stackCount;
            if (totalCount >= count)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 从容器中取出指定数量的物品
    /// </summary>
    /// <returns>取出的物品列表</returns>
    public static List<Thing> TakeThingsOfDef(IThingHolder thingHolder, ThingDef thingDef, int count, out int actualTakeCount, Predicate<Thing> validator = null)
    {
        actualTakeCount = 0;

        if (count <= 0 || thingHolder is null)
            return [];

        if (thingHolder is Map map)
            return OAFrame_MapUtility.TakeThingsOfDef(map, thingDef, count, out actualTakeCount, validator);

        if (thingHolder is Caravan caravan)
            return OAFrame_CaravanUtility.TakeThingsOfDef(caravan, thingDef, count, out actualTakeCount, validator);

        if (thingHolder is FixedCaravan fixedCaravan)
            return OAFrame_FixedCaravanUtility.TakeThingsOfDef(fixedCaravan, thingDef, count, out actualTakeCount, validator);

        ThingOwner directlyHeldThings = thingHolder.GetDirectlyHeldThings();
        List<Thing> takenThings = [];
        int remaining = count;
        bool hasValidator = validator is not null;
        for (int i = directlyHeldThings.Count - 1; i >= 0; i--)
        {
            Thing item = directlyHeldThings[i];
            if (item.def != thingDef && (hasValidator && !validator(item)))
                continue;

            int takeCount = Mathf.Min(remaining, item.stackCount);
            takenThings.Add(item.holdingOwner.Take(item, takeCount));
            if ((remaining -= takeCount) <= 0)
                break;
        }

        return takenThings;
    }

    /// <summary>
    /// 从容器中移除指定数量的物品（直接销毁）
    /// </summary>
    /// <returns>实际移除的数量</returns>
    public static int RemoveThingsOfDef(IThingHolder thingHolder, ThingDef thingDef, int count, Predicate<Thing> validator = null)
    {
        if (count <= 0 || thingHolder is null)
            return 0;

        if (thingHolder is Map map)
            return OAFrame_MapUtility.DestroyThingsOfDef(map, thingDef, count, validator);

        if (thingHolder is Caravan caravan)
            return OAFrame_CaravanUtility.RemoveThingsOfDef(caravan, thingDef, count, validator);

        if (thingHolder is FixedCaravan fixedCaravan)
            return OAFrame_FixedCaravanUtility.RemoveThingsOfDef(fixedCaravan, thingDef, count, validator);

        List<Thing> takeThings = TakeThingsOfDef(thingHolder, thingDef, count, out int actualTakeCount, validator);
        for (int i = takeThings.Count - 1; i >= 0; i--)
        {
            takeThings[i].Destroy();
        }

        return actualTakeCount;
    }

}
