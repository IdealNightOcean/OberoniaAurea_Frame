using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_MapUtility
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AmountSendableSilver(this Map map)
    {
        return AmountSendableThing(map, ThingDefOf.Silver);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AmountSendableThing(this Map map, ThingDef thingDef)
    {
        return (from t in TradeUtility.AllLaunchableThingsForTrade(map)
                where t.def == thingDef
                select t).Sum(t => t.stackCount);
    }

    public static bool HasEnoughThingsOfDef(this Map map, ThingDef thingDef, int count)
    {
        if (map is null || thingDef is null)
        {
            return false;
        }
        if (count <= 0)
        {
            return true;
        }

        List<Thing> things = map.listerThings.ThingsOfDef(thingDef);
        int curCount = 0;
        for (int i = 0; i < things.Count; i++)
        {
            curCount += things[i].stackCount;
            if (curCount >= count)
            {
                return true;
            }
        }
        return false;
    }

    public static bool HasEnoughThingsOfDef(this Map map, ThingDef thingDef, int count, Predicate<Thing> validator)
    {
        if (map is null || thingDef is null)
        {
            return false;
        }
        if (count <= 0)
        {
            return true;
        }
        if (validator is null)
        {
            return HasEnoughThingsOfDef(map, thingDef, count);
        }

        List<Thing> things = map.listerThings.ThingsOfDef(thingDef);
        int curCount = 0;
        for (int i = 0; i < things.Count; i++)
        {
            if (validator(things[i]))
            {
                curCount += things[i].stackCount;
                if (curCount >= count)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static List<Thing> TakeThingsOfDef(Map map, ThingDef thingDef, int count, out int actualTakeCount)
    {
        List<Thing> takeThings = [];
        actualTakeCount = 0;

        if (map is null || thingDef is null)
        {
            return takeThings;
        }
        if (count <= 0)
        {
            return takeThings;
        }

        List<Thing> things = map.listerThings.ThingsOfDef(thingDef);
        int remaining = count;

        for (int i = 0; i < things.Count; i++)
        {
            if (remaining >= things[i].stackCount)
            {
                remaining -= things[i].stackCount;
                takeThings.Add(things[i]);

            }
            else
            {
                takeThings.Add(things[i].SplitOff(remaining));
                remaining = 0;
            }

            if (remaining <= 0)
            {
                break;
            }
        }

        actualTakeCount = count - remaining;
        return takeThings;
    }

    public static List<Thing> TakeThingsOfDef(Map map, ThingDef thingDef, int count, Predicate<Thing> validator, out int actualTakeCount)
    {
        List<Thing> takeThings = [];
        actualTakeCount = 0;
        if (map is null || thingDef is null)
        {
            return takeThings;
        }
        if (count <= 0)
        {
            return takeThings;
        }
        if (validator is null)
        {
            return TakeThingsOfDef(map, thingDef, count, out actualTakeCount);
        }

        List<Thing> things = map.listerThings.ThingsOfDef(thingDef);
        int remaining = count;

        for (int i = 0; i < things.Count; i++)
        {
            if (!validator(things[i]))
            {
                continue;
            }

            if (remaining >= things[i].stackCount)
            {
                remaining -= things[i].stackCount;
                takeThings.Add(things[i]);

            }
            else
            {
                takeThings.Add(things[i].SplitOff(remaining));
                remaining = 0;
            }

            if (remaining <= 0)
            {
                break;
            }
        }

        actualTakeCount = count - remaining;
        return takeThings;
    }

    /// <returns>实际销毁数</returns>
    public static int DestoryThingsOfDef(this Map map, ThingDef thingDef, int count)
    {
        List<Thing> takeThings = TakeThingsOfDef(map, thingDef, count, out int actualDestoryCount);
        for (int i = takeThings.Count - 1; i >= 0; i--)
        {
            takeThings[i].Destroy();
        }
        return actualDestoryCount;
    }

    /// <returns>实际销毁数</returns>
    public static int DestoryThingsOfDef(this Map map, ThingDef thingDef, int count, Predicate<Thing> validator)
    {
        List<Thing> takeThings = TakeThingsOfDef(map, thingDef, count, validator, out int actualDestoryCount);
        for (int i = takeThings.Count - 1; i >= 0; i--)
        {
            takeThings[i].Destroy();
        }
        return actualDestoryCount;
    }

    /// <summary>
    /// 地图上派系威胁的数量
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ThreatsCountOfFaction(this Map map, Faction faction)
    {
        IEnumerable<Verse.AI.IAttackTarget> potentiallyDangerous = map.attackTargetsCache.TargetsHostileToFaction(faction).Where(t => GenHostility.IsActiveThreatTo(t, faction));
        return potentiallyDangerous.Count();
    }

    /// <summary>
    /// 地图上玩家派系威胁的数量
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ThreatsCountOfPlayer(this Map map)
    {
        return ThreatsCountOfFaction(map, Faction.OfPlayer);
    }

    /// <summary>
    /// 地图上派系敌人的数量
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int HostilePawnsCountOfFaction(this Map map, Faction faction)
    {
        IEnumerable<Pawn> potentiallyDangerous = map.mapPawns.AllPawnsSpawned.Where(p => !p.DeadOrDowned && !p.IsPrisoner && !p.InContainerEnclosed && p.Faction != faction && p.HostileTo(faction));
        return potentiallyDangerous.Count();
    }

    /// <summary>
    /// 地图上玩家敌人的数量
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int HostilePawnsCountOfPlayer(this Map map)
    {
        return HostilePawnsCountOfFaction(map, Faction.OfPlayer);
    }

}