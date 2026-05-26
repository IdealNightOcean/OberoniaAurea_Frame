using RimWorld;
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
}
