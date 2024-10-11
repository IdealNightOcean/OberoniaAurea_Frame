﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
public static class OAFrame_MapUtility
{
    //地图上派系威胁的数量
    public static int ThreatsCountOfFactionOnMap(Map map, Faction faction)
    {
        IEnumerable<Verse.AI.IAttackTarget> potentiallyDangerous = map.attackTargetsCache.TargetsHostileToFaction(faction).Where(t => GenHostility.IsActiveThreatTo(t, faction));
        return potentiallyDangerous.Count();
    }
    //地图上玩家派系威胁的数量
    public static int ThreatsCountOfPlayerOnMap(Map map)
    {
        return ThreatsCountOfFactionOnMap(map, Faction.OfPlayer);
    }

    //地图上派系敌人的数量
    public static int HostilePawnsCountOfFactionOnMap(Map map, Faction faction)
    {
        IEnumerable<Pawn> potentiallyDangerous = map.mapPawns.AllPawnsSpawned.Where(p => !p.DeadOrDowned && !p.IsPrisoner && !p.InContainerEnclosed && p.Faction != faction && p.HostileTo(faction));
        return potentiallyDangerous.Count();
    }
    //地图上玩家敌人的数量
    public static int HostilePawnsCountOfPlayerOnMap(Map map)
    {
        return HostilePawnsCountOfFactionOnMap(map, Faction.OfPlayer);
    }

}
