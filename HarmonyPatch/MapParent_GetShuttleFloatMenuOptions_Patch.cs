using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 为地图父对象添加穿梭机浮动菜单选项的补丁。
/// </summary>
[StaticConstructorOnStartup]
[HarmonyPatch(typeof(MapParent), "GetShuttleFloatMenuOptions")]
public class MapParent_GetShuttleFloatMenuOptions_Patch
{
    /// <summary>
    /// 在原有浮动菜单选项基础上附加组建商队的选项。
    /// </summary>
    /// <param name="origin">原有浮动菜单选项集合。</param>
    /// <param name="___tile">当前地图所在星球地块。</param>
    /// <param name="pods">包含的物体持有者集合。</param>
    /// <param name="launchAction">用于发射穿梭机的动作。</param>
    /// <returns>最终的浮动菜单选项集合。</returns>
    [HarmonyPostfix]
    public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> origin, PlanetTile ___tile, IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
    {
        foreach (FloatMenuOption menuOption in origin)
        {
            yield return menuOption;
        }

        if (TransportersArrivalAction_FormCaravan.CanFormCaravanAt(pods, ___tile))
        {
            yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate
            {
                launchAction(___tile, new TransportersArrivalAction_FormCaravan("MessageShuttleArrived"));
            });
        }
    }
}
