using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary> 
/// 固定远行队工具类。 
/// </summary>
public static class OAFrame_FixedCaravanUtility
{
    private static readonly List<Thing> TempInventoryItems = [];
    private static readonly List<Thing> TempAddedItems = [];
    private static readonly List<Pawn> TempPawns = [];

    /// <summary>
    /// 获取Pawn所在的固定远行队。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedCaravan GetFixedCaravan(this Pawn pawn)
    {
        return pawn.ParentHolder as FixedCaravan;
    }

    /// <summary>
    /// 获取固定远行队的所有库存物品。
    /// </summary>
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

    /// <summary>
    /// 从远行队创建固定远行队。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedCaravan CreateFixedCaravan(Caravan caravan)
    {
        return CreateFixedCaravan(caravan, OAFrameDefOf.OAFrame_FixedCaravan);
    }

    /// <summary>
    /// 从远行队创建指定def的固定远行队。
    /// </summary>
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

    /// <summary>
    /// 从远行队创建关联世界对象的固定远行队。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedCaravan CreateFixedCaravan(Caravan caravan, WorldObject worldObject)
    {
        return CreateFixedCaravan(caravan, OAFrameDefOf.OAFrame_FixedCaravan, worldObject);
    }

    /// <summary>
    /// 从远行队创建指定def并关联世界对象的固定远行队。
    /// </summary>
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

    /// <summary>
    /// 将固定远行队转换为远行队。
    /// </summary>
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

    /// <summary>
    /// 向固定远行队批量添加物品。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GiveThings(FixedCaravan fixedCaravan, IEnumerable<Thing> things)
    {
        foreach (Thing t in things)
        {
            GiveThing(fixedCaravan, t);
        }
    }

    /// <summary>
    /// 向固定远行队添加单个物品。
    /// </summary>
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

    /// <summary>
    /// 向固定远行队批量添加<see cref="Pawn"/>或物品。
    /// </summary>
    public static void GivePawnsOrThings(FixedCaravan fixedCaravan, IEnumerable<Thing> things)
    {
        TempAddedItems.Clear();
        TempAddedItems.AddRange(things);
        for (int i = 0; i < TempAddedItems.Count; i++)
        {
            fixedCaravan.AddPawnOrItem(TempAddedItems[i]);
        }
        TempAddedItems.Clear();
    }

    /// <summary>
    /// 获取固定远行队中指定def物品的数量。
    /// </summary>
    public static int GetCountOfThingDef(this FixedCaravan fixedCaravan, ThingDef thingDef, Predicate<Thing> validator = null)
    {
        int totalCount = 0;
        List<Thing> caravanInventory = AllInventoryItems(fixedCaravan);
        bool hasValidator = validator is not null;
        Thing item;

        for (int i = 0; i < caravanInventory.Count; i++)
        {
            item = caravanInventory[i];
            if (item.def == thingDef && (!hasValidator || validator(item)))
            {
                totalCount += item.stackCount;
            }
        }

        return totalCount;
    }

    public static bool HasThingsOfDef(this FixedCaravan fixedCaravan, ThingDef thingDef, int count, Predicate<Thing> validator = null)
    {
        if (count <= 0)
            return true;
        else
            return GetCountOfThingDef(fixedCaravan, thingDef, validator) >= count;
    }

    /// <summary>
    /// 从固定远行队取出指定def物品的物品。
    /// </summary>
    public static List<Thing> TakeThingsOfDef(FixedCaravan fixedCaravan, ThingDef thingDef, int count, out int actualTakeCount, Predicate<Thing> validator = null)
    {
        actualTakeCount = 0;
        if (count <= 0 || fixedCaravan is null || thingDef is null)
            return [];

        List<Thing> caravanInventory = AllInventoryItems(fixedCaravan);

        int remaining = count;
        List<Thing> takeThings = [];
        bool hasValidator = validator is not null;

        for (int i = caravanInventory.Count - 1; i >= 0; i--)
        {
            Thing item = caravanInventory[i];
            if (item.def != thingDef || (hasValidator && !validator(item)))
                continue;

            int takeCount = Mathf.Min(remaining, item.stackCount);
            takeThings.Add(item.holdingOwner.Take(item, takeCount));
            if ((remaining -= takeCount) <= 0)
                break;
        }

        actualTakeCount = count - remaining;
        return takeThings;
    }

    /// <summary>
    /// 移除固定远行队中指定def物品的物品。
    /// </summary>
    /// <returns>实际移除数</returns>
    public static int RemoveThingsOfDef(this FixedCaravan fixedCaravan, ThingDef thingDef, int count, Predicate<Thing> validator = null)
    {
        List<Thing> takeThings = TakeThingsOfDef(fixedCaravan, thingDef, count, out int actualTakeCount, validator);
        for (int i = takeThings.Count - 1; i >= 0; i--)
        {
            takeThings[i].Destroy();
        }

        return actualTakeCount;
    }

    /// <summary>
    /// 尝试以多种安全方式向玩家方发放物品（优先目标：固定远行队 → 地图上玩家远行队 → 随机玩家基地 → 任意玩家远行队）
    /// </summary>
    /// <param name="worldObject">用于定位固定远行队的世界对象</param>
    /// <param name="things">需要发放的物品集合</param>
    /// <param name="oriTile">优先查找远行队的目标地图格子（为空则使用 worldObject.Tile）</param>
    /// <param name="errorOnFailed">发放失败时是否记录错误日志</param>
    /// <returns>是否成功发放物品</returns>
    public static bool TryGiveThingsToPlayer(WorldObject_InteractWithFixedCaravanBase worldObject, IEnumerable<Thing> things, PlanetTile? oriTile = null, bool errorOnFailed = false)
    {
        FixedCaravan fixedCaravan = worldObject.AssociatedFixedCaravan;
        if (fixedCaravan is not null && fixedCaravan.Spawned)
        {
            OAFrame_FixedCaravanUtility.GiveThings(fixedCaravan, things);
            return true;
        }

        Caravan caravan = OAFrame_CaravanUtility.GetPlayerCaravanOnTile(oriTile ?? worldObject.Tile);
        if (caravan is not null && caravan.Spawned)
        {
            OAFrame_CaravanUtility.GiveThings(caravan, things);
            return true;
        }

        Map map = Find.RandomPlayerHomeMap;
        if (map is not null)
        {
            OAFrame_DropPodUtility.DefaultDropThing(things, map);
            return true;
        }

        caravan = Find.WorldObjects.Caravans.Where(c => c.Faction.IsPlayerSafe() && c.Spawned).RandomElementWithFallback();
        if (caravan is not null)
        {
            OAFrame_CaravanUtility.GiveThings(caravan, things);
            return true;
        }

        if (errorOnFailed)
            Log.Error("[OAFrame] 无法通过任何一般方法向给予玩家物品。");

        return false;
    }

}